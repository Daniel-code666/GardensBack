using AutoMapper;
using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.DeviceConnectivity.Abstractions;
using GardensBack.Application.FirmwareReleases.Constants;
using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Application.FirmwareUpdates.Configuration;
using GardensBack.Application.FirmwareUpdates.Constants;
using GardensBack.Application.FirmwareUpdates.Dtos;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace GardensBack.Application.FirmwareUpdates.Services
{
    public class DeviceFirmwareUpdateService(
        IDeviceFirmwareUpdateRepository repository,
        IFirmwareReleaseRepository firmwareReleaseRepository,
        IDeviceConnectivityConfigurationRepository connectivityRepository,
        IDeviceCommandKeyProtector commandKeyProtector,
        IOtaCommandAuthenticationService authenticationService,
        IDeviceControlPublisher controlPublisher,
        IMapper mapper,
        IOptions<FirmwareUpdateOptions> options) : IDeviceFirmwareUpdateService
    {
        private static readonly Regex SemanticVersionPattern = new(@"^\d+\.\d+\.\d+$", RegexOptions.CultureInvariant);
        private readonly IDeviceFirmwareUpdateRepository _repository = repository;
        private readonly IFirmwareReleaseRepository _firmwareReleaseRepository = firmwareReleaseRepository;
        private readonly IDeviceConnectivityConfigurationRepository _connectivityRepository = connectivityRepository;
        private readonly IDeviceCommandKeyProtector _commandKeyProtector = commandKeyProtector;
        private readonly IOtaCommandAuthenticationService _authenticationService = authenticationService;
        private readonly IDeviceControlPublisher _controlPublisher = controlPublisher;
        private readonly IMapper _mapper = mapper;
        private readonly FirmwareUpdateOptions _options = options.Value;

        public async Task<DeviceFirmwareUpdateResponse> RequestUpdate(RequestDeviceFirmwareUpdateRequest request, int requestedByUserId, CancellationToken ct)
        {
            request.Validate();

            if (requestedByUserId <= 0)
                throw new UnathorizedException("La identidad del usuario no es válida.");

            MicrocontrollerDevice device = await _repository.FindDevice(request.DeviceId, ct)
                ?? throw new NotFoundException("El dispositivo no fue encontrado.");

            ValidateDevice(device, requestedByUserId);

            FirmwareRelease release = await _firmwareReleaseRepository.FindByReleaseId(request.ReleaseId, ct)
                ?? throw new NotFoundException("La publicación de firmware no fue encontrada.");

            ValidateRelease(device, release);

            if (await _repository.FindInProgress(device.Id, ct) is not null)
                throw new ConflictException("El dispositivo ya tiene una actualización de firmware en curso.");

            DeviceConnectivityConfiguration connectivity = await _connectivityRepository.GetDefault(ct)
                ?? throw new ValidationException("La configuración de conectividad para dispositivos no ha sido creada.");

            if (!connectivity.Active)
                throw new ValidationException("La configuración de conectividad para dispositivos no está activa.");

            string protectedCommandKey = device.MqttCredential?.ProtectedCommandKey
                ?? throw new ValidationException("El dispositivo no tiene registrada la clave necesaria para autenticar comandos remotos.");
            byte[] commandKey = _commandKeyProtector.Unprotect(protectedCommandKey);

            try
            {
                uint lastCommandVersion = await _repository.GetLastCommandVersion(device.Id, ct);
                if (lastCommandVersion == uint.MaxValue)
                    throw new ConflictException("La secuencia de comandos OTA alcanzó el valor máximo permitido.");

                DateTime now = DateTime.UtcNow;
                DateTime expiresAt = now.AddMinutes(_options.CommandExpirationMinutes);
                uint commandVersion = lastCommandVersion + 1;
                string requestId = Guid.NewGuid().ToString("N");

                var update = new DeviceFirmwareUpdate
                {
                    RequestId = requestId,
                    CommandVersion = commandVersion,
                    Status = FirmwareUpdateStatus.Pending,
                    PreviousVersion = device.FirmwareVersion!,
                    RequestedAt = now,
                    ExpiresAt = expiresAt,
                    ProgressPercentage = 0,
                    MicroCtlDeviceId = device.Id,
                    MicrocontrollerDevice = device,
                    FirmwareReleaseId = release.Id,
                    FirmwareRelease = release,
                    RequestedByUserId = requestedByUserId,
                    RequestedByUser = device.User
                };

                await _repository.Add(update, ct);
                await _repository.SaveChanges(ct);

                try
                {
                    OtaUpdateCommand command = BuildCommand(device, release, connectivity, update, expiresAt);
                    command.AuthenticationTag = _authenticationService.Sign(command, commandKey);
                    await _controlPublisher.Publish(device.DeviceId!, command, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception)
                {
                    update.Status = FirmwareUpdateStatus.Failed;
                    update.CompletedAt = DateTime.UtcNow;
                    update.FailureCode = "mqtt_publish_failed";
                    update.FailureMessage = "No fue posible publicar la solicitud de actualización.";
                    await _repository.SaveChanges(CancellationToken.None);
                    throw;
                }

                return _mapper.Map<DeviceFirmwareUpdateResponse>(update);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(commandKey);
            }
        }

        private static void ValidateDevice(MicrocontrollerDevice device, int requestedByUserId)
        {
            if (!device.Active)
                throw new ValidationException("El dispositivo no está activo.");

            if (device.UserId != requestedByUserId)
                throw new UnathorizedException("El dispositivo no pertenece al usuario autenticado.");

            if (device.ProvisioningState != ProvisioningState.Provisioned)
                throw new ValidationException("El dispositivo no está provisionado.");

            if (string.IsNullOrWhiteSpace(device.DeviceId) || string.IsNullOrWhiteSpace(device.Board) || string.IsNullOrWhiteSpace(device.FirmwareVersion))
                throw new ValidationException("El dispositivo no tiene completa la información requerida para actualizar el firmware.");
        }

        private static void ValidateRelease(MicrocontrollerDevice device, FirmwareRelease release)
        {
            if (!release.Active || release.Status != FirmwareReleaseStatus.Published)
                throw new ValidationException("La publicación de firmware no está disponible.");

            if (!string.Equals(device.Board, release.Board, StringComparison.Ordinal))
                throw new ValidationException("La publicación de firmware no corresponde a la placa del dispositivo.");

            Version currentVersion = ParseVersion(device.FirmwareVersion!, "La versión actual del dispositivo no es válida.");
            Version targetVersion = ParseVersion(release.Version, "La versión de la publicación no es válida.");

            if (targetVersion <= currentVersion)
                throw new ConflictException("La versión de firmware solicitada debe ser superior a la instalada.");
        }

        private static Version ParseVersion(string value, string errorMessage)
        {
            if (!SemanticVersionPattern.IsMatch(value) || !Version.TryParse(value, out Version? version))
                throw new ValidationException(errorMessage);

            return version;
        }

        private static OtaUpdateCommand BuildCommand(MicrocontrollerDevice device, FirmwareRelease release, DeviceConnectivityConfiguration connectivity, DeviceFirmwareUpdate update, DateTime expiresAt)
        {
            string scheme = connectivity.ApiUseTls ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            string manifestPath = $"{FirmwareReleaseContract.ControllerRoute}/{Uri.EscapeDataString(release.ReleaseId)}/manifest";
            string manifestUrl = new UriBuilder(scheme, connectivity.ApiHost, connectivity.ApiPort, manifestPath).Uri.AbsoluteUri;

            return new OtaUpdateCommand
            {
                Type = FirmwareUpdateContract.UpdateRequestType,
                ContractVersion = FirmwareUpdateContract.Version,
                RequestId = update.RequestId,
                DeviceId = device.DeviceId!,
                CommandVersion = update.CommandVersion,
                ReleaseId = release.ReleaseId,
                ManifestUrl = manifestUrl,
                ExpiresAt = expiresAt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
                AuthenticationTag = string.Empty
            };
        }

    }
}
