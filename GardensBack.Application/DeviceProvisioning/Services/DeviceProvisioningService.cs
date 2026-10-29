using GardensBack.Application.DeviceProvisioning.Abstractions;
using GardensBack.Application.DeviceProvisioning.Dtos.Web;

namespace GardensBack.Application.DeviceProvisioning.Services
{
    using AutoMapper;
    using GardensBack.Application.Abstractions;
    using GardensBack.Application.Common.Exceptions;
    using GardensBack.Application.DeviceConnectivity.Abstractions;
    using GardensBack.Application.DeviceProvisioning.Configuration;
    using GardensBack.Application.DeviceProvisioning.Constants;
    using GardensBack.Application.DeviceProvisioning.Dtos;
    using GardensBack.Application.DeviceProvisioning.Dtos.Device;
    using GardensBack.Application.DeviceProvisioning.Dtos.Internal;
    using GardensBack.Domain.Entities;
    using GardensBack.Domain.Enums;
    using Microsoft.Extensions.Options;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    public class DeviceProvisioningService(
        IDeviceProvisioningRepository deviceRepository,
        IUserRepository userRepository,
        IProvisioningTokenService tokenService,
        IDeviceConnectivityConfigurationRepository connectivityRepository,
        IClaimResponseSigner claimResponseSigner,
        IMqttCredentialEnvelopeService mqttCredentialEnvelopeService,
        IMapper mapper,
        IOptions<DeviceProvisioningOptions> options,
        IOptions<RabbitMqOptions> rabbitMqOptions) : IDeviceProvisioningService
    {
        private readonly IDeviceProvisioningRepository _deviceRepository = deviceRepository;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IProvisioningTokenService _tokenService = tokenService;
        private readonly IDeviceConnectivityConfigurationRepository _connectivityRepository = connectivityRepository;
        private readonly IMapper _mapper = mapper;
        private readonly DeviceProvisioningOptions _options = options.Value;
        private readonly IClaimResponseSigner _claimResponseSigner = claimResponseSigner;
        private readonly IMqttCredentialEnvelopeService _mqttCredentialEnvelopeService = mqttCredentialEnvelopeService;
        private readonly RabbitMqOptions _rabbitMqOptions = rabbitMqOptions.Value;

        public async Task<StartDeviceProvisioningResponse> StartProvisioning(int userId, StartDeviceProvisioningRequest request, CancellationToken ct)
        {
            request.Validate();

            if (userId <= 0)
                throw new UnathorizedException("La identidad del usuario no es válida.");

            if (request.ProvisioningState == ProvisioningState.Provisioned)
                throw new ValidationException("El dispositivo ya se encuentra provisionado.");

            DeviceConnectivityConfiguration connectivity = await _connectivityRepository.GetDefault(ct)
                ?? throw new ValidationException("La configuración de conectividad para dispositivos no ha sido creada.");

            if (!connectivity.Active)
                throw new ValidationException("La configuración de conectividad para dispositivos no está activa.");

            string apiScheme = connectivity.ApiUseTls ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            string serviceBaseUrl = new UriBuilder(apiScheme, connectivity.ApiHost, connectivity.ApiPort)
                .Uri
                .GetLeftPart(UriPartial.Authority)
                .TrimEnd('/');

            var foundUser = await _userRepository.GetUserById(userId, ct);
            if (!foundUser.Successful || foundUser.Result is null)
                throw new NotFoundException("Usuario no encontrado.");

            IReadOnlyList<MicrocontrollerDevice> identityMatches =
                await _deviceRepository.FindIdentityMatches(
                    request.DeviceId,
                    request.HardwareId,
                    request.MqttClientId,
                    ct);

            if (identityMatches.Count > 1)
                throw new ValidationException("Los identificadores pertenecen a dispositivos diferentes.");

            MicrocontrollerDevice? device = identityMatches.SingleOrDefault();
            if (device is not null &&
                (device.DeviceId != request.DeviceId ||
                 device.HardwareId != request.HardwareId ||
                 device.MqttClientId != request.MqttClientId))
            {
                throw new ValidationException("La identidad reportada no coincide con el dispositivo registrado.");
            }

            if (device is not null && device.UserId != userId)
                throw new UnathorizedException("El dispositivo pertenece a otro usuario.");

            if (device is not null && device.ProvisioningState == ProvisioningState.Provisioned)
                throw new ValidationException("El dispositivo ya se encuentra provisionado.");

            DateTime now = DateTime.UtcNow;
            DateTime expiresAtUtc = now.AddMinutes(_options.TokenExpirationMinutes);
            string bootstrapRequestId = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
            GeneratedProvisioningToken generatedToken = _tokenService.Generate();

            if (device is null)
            {
                device = _mapper.Map<MicrocontrollerDevice>(request);
                device.UserId = userId;
                device.User = foundUser.Result;

                await _deviceRepository.AddDevice(device, ct);
            }
            else
            {
                _mapper.Map(request, device);
            }

            device.ProvisioningState = ProvisioningState.Unprovisioned;

            DeviceClaim claim = device.DeviceClaim ?? new DeviceClaim
            {
                MicroCtrlDevice = device,
                MicrocontrollerDeviceId = device.Id
            };

            claim.BootstrapRequestId = bootstrapRequestId;
            claim.ProvisioningTokenHash = generatedToken.Hash;
            claim.ProvisioningTokenExpiresAt = expiresAtUtc;
            claim.ProvisioningTokenConsumedAt = null;
            claim.RequestId = null;
            claim.ClaimCodeHash = null;
            claim.ResponseAuthenticationTag = null;
            claim.ContractVersion = request.ContractVersion;
            claim.DeviceClaimStatus = DeviceClaimStatus.Pending;
            claim.RequestedAt = now;
            claim.ExpiresAt = expiresAtUtc;
            claim.ResolvedAt = null;
            claim.ResolvedByUserId = null;
            claim.ResolvedByUser = null;
            claim.ErrorCode = null;
            claim.AttemptCount = 0;
            claim.Active = true;
            device.DeviceClaim = claim;

            var response = new StartDeviceProvisioningResponse
            {
                DevicePublicId = device.PublicId,
                DeviceId = device.DeviceId!,
                BootstrapRequestId = bootstrapRequestId,
                ContractVersion = request.ContractVersion,
                ServiceBaseUrl = serviceBaseUrl.TrimEnd('/'),
                ProvisioningToken = generatedToken.Token,
                ExpiresAtUtc = expiresAtUtc
            };

            response.Validate();
            await _deviceRepository.SaveChanges(ct);

            return response;
        }

        public async Task<ClaimResponse> RegisterClaim(string provisioningToken, ClaimRegistrationRequest request, CancellationToken ct)
        {
            request.Validate();

            var hash = _tokenService.ComputeHash(provisioningToken);

            if (await _deviceRepository.FindClaimByTokenHash(hash, ct) is DeviceClaim deviceClaim)
            {
                DateTime now = DateTime.UtcNow;

                VerifyToken(provisioningToken, deviceClaim, now);

                VerifyDevice(request, deviceClaim);

                ClaimResponse? existingResponse = ValidateClaimState(request, deviceClaim);

                if (existingResponse is not null)
                    return existingResponse;

                _mapper.Map(request, deviceClaim);

                UpdateDeviceClaim(deviceClaim, request, now);

                ClaimResponse response = _mapper.Map<ClaimResponse>(deviceClaim);

                response.AuthenticationTag = _claimResponseSigner.Sign(response, request.ClaimCode);
                response.Validate();

                deviceClaim.ResponseAuthenticationTag = response.AuthenticationTag;

                await _deviceRepository.SaveChanges(ct);

                return response;
            }

            throw new UnathorizedException("El token de aprovisionamiento no es válido.");
        }

        public async Task<ClaimResponse?> GetClaim(string provisioningToken, string requestId, CancellationToken ct)
        {
            string tokenHash = _tokenService.ComputeHash(provisioningToken);

            if (await _deviceRepository.FindClaimByTokenHash(tokenHash, ct) is DeviceClaim deviceClaim && deviceClaim.Active)
            {
                DateTime now = DateTime.UtcNow;

                VerifyToken(provisioningToken, deviceClaim, now);

                if (deviceClaim.RequestId != requestId)
                    throw new NotFoundException("Claim del dispositivo no encontrado.");

                VerifyApprovedClaimResponse(deviceClaim);

                ClaimResponse response = _mapper.Map<ClaimResponse>(deviceClaim);
                response.Validate();

                return response;
            }

            throw new UnathorizedException("El token de aprovisionamiento no es válido.");
        }

        public async Task<MqttCredentialsEnvelopeResponse> RegisterMqttCredentials(string provisioningToken, MqttCredentialsRegistrationRequest request, CancellationToken ct)
        {
            request.Validate();

            string tokenHash = _tokenService.ComputeHash(provisioningToken);
            DeviceClaim deviceClaim = await _deviceRepository.FindClaimByTokenHash(tokenHash, ct) ?? throw new UnathorizedException("El token de aprovisionamiento no es válido.");
            DateTime now = DateTime.UtcNow;

            VerifyTokenIdentityAndLifetime(provisioningToken, deviceClaim, now);
            VerifyMqttCredentialRequest(request, deviceClaim);

            MicrocontrollerDevice device = deviceClaim.MicroCtrlDevice;
            MqttCredential? credential = device.MqttCredential;

            if (credential?.RequestId == request.RequestId)
                return GetStoredMqttCredentialResponse(credential);

            if (deviceClaim.ProvisioningTokenConsumedAt is not null)
                throw new ConflictException("El token ya fue consumido por otra solicitud de credenciales.");

            ValidateCredentialVersion(request.CurrentCredentialVersion, credential);

            DeviceConnectivityConfiguration connectivity = await _connectivityRepository.GetDefault(ct)
                ?? throw new ValidationException("La configuración de conectividad para dispositivos no ha sido creada.");

            if (!connectivity.Active)
                throw new ValidationException("La configuración de conectividad para dispositivos no está activa.");

            uint credentialVersion = checked(request.CurrentCredentialVersion + 1);
            var payload = new MqttCredentialsPayload
            {
                Host = connectivity.MqttHost,
                Port = connectivity.MqttPort,
                ClientId = device.MqttClientId!,
                User = _rabbitMqOptions.Username,
                Password = _rabbitMqOptions.Password,
                CredentialVersion = credentialVersion
            };
            payload.Validate();

            MqttCredentialsEnvelopeResponse response = _mqttCredentialEnvelopeService.Encrypt(
                payload, request.DeviceId, request.RequestId, request.BootstrapKey);

            if (credential is null)
            {
                credential = new MqttCredential
                {
                    MicrocontrollerDeviceId = device.Id,
                    MicroCtrlDevice = device,
                    UserName = payload.User,
                    Host = payload.Host,
                    SecretReference = "RabbitMQ:Password"
                };
                await _deviceRepository.AddMqttCredential(credential, ct);
                device.MqttCredential = credential;
            }

            UpdateMqttCredential(credential, payload, request, response, now);
            deviceClaim.ProvisioningTokenConsumedAt = now;

            await _deviceRepository.SaveChanges(ct);
            return response;
        }

        public async Task<MqttCredentialsEnvelopeResponse> GetMqttCredentials(string provisioningToken, string requestId, CancellationToken ct)
        {
            if (!Regex.IsMatch(requestId, DeviceProvisioningContract.RequestIdPattern))
                throw new ValidationException("El RequestId de credenciales no es válido.");

            string tokenHash = _tokenService.ComputeHash(provisioningToken);
            DeviceClaim deviceClaim = await _deviceRepository.FindClaimByTokenHash(tokenHash, ct)
                ?? throw new UnathorizedException("El token de aprovisionamiento no es válido.");

            VerifyTokenIdentityAndLifetime(provisioningToken, deviceClaim, DateTime.UtcNow);
            VerifyApprovedClaimResponse(deviceClaim);

            MqttCredential? credential = deviceClaim.MicroCtrlDevice.MqttCredential;
            if (credential is null || credential.RequestId != requestId)
                throw new NotFoundException("Solicitud de credenciales MQTT no encontrada.");

            return GetStoredMqttCredentialResponse(credential);
        }

        /// <summary>
        /// Verificaiones sobre el token, si falla cualquiera devuelve excepción
        /// </summary>
        /// <param name="provisioningToken"></param>
        /// <param name="deviceClaim"></param>
        /// <exception cref="ValidationException"></exception>
        private void VerifyToken(string provisioningToken, DeviceClaim deviceClaim, DateTime now)
        {
            VerifyTokenIdentityAndLifetime(provisioningToken, deviceClaim, now);

            if (deviceClaim.ProvisioningTokenConsumedAt is not null)
                throw new ValidationException("El token de aprovisionamiento ya fue consumido.");
        }

        private void VerifyTokenIdentityAndLifetime(string provisioningToken, DeviceClaim deviceClaim, DateTime now)
        {
            if (!deviceClaim.Active)
                throw new ValidationException("El claim del dispositivo no está activo.");

            if (deviceClaim.ProvisioningTokenExpiresAt is null || deviceClaim.ProvisioningTokenExpiresAt.Value <= now)
                throw new UnathorizedException("El token de aprovisionamiento expiró.");

            if (deviceClaim.ProvisioningTokenHash is not string provisioningTokenHash || !_tokenService.Verify(provisioningToken, provisioningTokenHash))
                throw new UnathorizedException("El token de aprovisionamiento no es válido.");
        }

        /// <summary>
        /// Verificaciones sobre el dispositivo
        /// </summary>
        /// <param name="request"></param>
        /// <param name="deviceClaim"></param>
        /// <exception cref="ValidationException"></exception>
        private static void VerifyDevice(ClaimRegistrationRequest request, DeviceClaim deviceClaim)
        {
            if (request.DeviceId != deviceClaim.MicroCtrlDevice.DeviceId)
                throw new ValidationException("El id del dispositivo asociado no coincide con el dispositivo registrado");

            if (request.HardwareId != deviceClaim.MicroCtrlDevice.HardwareId)
                throw new ValidationException("El id del hardware del dispositivo asociado no coincide con el dispositivo registrado");

            if (request.MqttClientId != deviceClaim.MicroCtrlDevice.MqttClientId)
                throw new ValidationException("El id del cliente MQTT del dispositivo asociado no coincide con el dispositivo registrado");

            if (request.Board != deviceClaim.MicroCtrlDevice.Board)
                throw new ValidationException("El id de la tarjeta del dispositivo asociado no coincide con el dispositivo registrado");

            if (request.FirmwareVersion != deviceClaim.MicroCtrlDevice.FirmwareVersion)
                throw new ValidationException("El id del firmware del dispositivo asociado no coincide con el dispositivo registrado");
        }

        private static void VerifyMqttCredentialRequest(MqttCredentialsRegistrationRequest request, DeviceClaim deviceClaim)
        {
            VerifyApprovedClaimResponse(deviceClaim);

            if (request.ContractVersion != deviceClaim.ContractVersion)
                throw new ConflictException("La versión del contrato no coincide con la utilizada durante el claim.");

            if (request.DeviceId != deviceClaim.MicroCtrlDevice.DeviceId)
                throw new ValidationException("El dispositivo de la solicitud de credenciales no coincide con el claim.");

            if (string.IsNullOrWhiteSpace(deviceClaim.MicroCtrlDevice.MqttClientId))
                throw new ValidationException("El dispositivo no tiene un identificador de cliente MQTT.");
        }

        private static void ValidateCredentialVersion(uint currentCredentialVersion, MqttCredential? credential)
        {
            if (currentCredentialVersion >= int.MaxValue)
                throw new ValidationException("La versión de credenciales alcanzó el valor máximo permitido.");

            if (credential is null && currentCredentialVersion != 0)
                throw new ConflictException("El dispositivo reportó una versión MQTT que no existe en el backend.");

            if (credential is not null && (credential.Version < 0 || currentCredentialVersion != (uint)credential.Version))
                throw new ConflictException("La versión MQTT reportada no coincide con la registrada en el backend.");
        }

        private static void UpdateMqttCredential(MqttCredential credential, MqttCredentialsPayload payload, MqttCredentialsRegistrationRequest request, MqttCredentialsEnvelopeResponse response, DateTime now)
        {
            credential.Version = checked((int)payload.CredentialVersion);
            credential.RequestId = request.RequestId;
            credential.UserName = payload.User;
            credential.MqttClientId = payload.ClientId;
            credential.Host = payload.Host;
            credential.Port = payload.Port;
            credential.SecretReference = "RabbitMQ:Password";
            credential.Status = MqttCredentialStatus.Testing;
            credential.IssuedAt = now;
            credential.ActivatedAt = null;
            credential.RevokedAt = null;
            credential.FailureCode = null;
            credential.ResponseNonce = response.Nonce;
            credential.ResponseCiphertext = response.Ciphertext;
            credential.ResponseAuthenticationTag = response.AuthenticationTag;
            credential.Active = true;
        }

        private MqttCredentialsEnvelopeResponse GetStoredMqttCredentialResponse(MqttCredential credential)
        {
            if (string.IsNullOrWhiteSpace(credential.RequestId) ||
                string.IsNullOrWhiteSpace(credential.ResponseNonce) ||
                string.IsNullOrWhiteSpace(credential.ResponseCiphertext) ||
                string.IsNullOrWhiteSpace(credential.ResponseAuthenticationTag))
            {
                throw new ValidationException("La respuesta de credenciales MQTT almacenada está incompleta.");
            }

            MqttCredentialsEnvelopeResponse response = _mapper.Map<MqttCredentialsEnvelopeResponse>(credential);
            response.Validate();
            return response;
        }

        /// <summary>
        /// Computa el hash
        /// </summary>
        /// <param name="claimCode"></param>
        /// <returns></returns>
        private static string ComputeClaimCodeHash(string claimCode)
        {
            byte[] value = Encoding.UTF8.GetBytes(claimCode);
            byte[] hash = SHA256.HashData(value);

            return Convert.ToHexStringLower(hash);
        }

        /// <summary>
        /// Devuelve el tiempo de expiración del token
        /// </summary>
        /// <param name="now"></param>
        /// <param name="expiresInSeconds"></param>
        /// <param name="tokenExpiresAt"></param>
        /// <returns></returns>
        private static DateTime GetClaimExpiration(DateTime now, int expiresInSeconds, DateTime? tokenExpiresAt)
        {
            DateTime requestedExpiration = now.AddSeconds(expiresInSeconds);

            if (tokenExpiresAt is null)
                return requestedExpiration;

            return requestedExpiration <= tokenExpiresAt.Value ? requestedExpiration : tokenExpiresAt.Value;
        }

        /// <summary>
        /// Actualiza los campos sobre la entidad trackeada por EF
        /// </summary>
        /// <param name="deviceClaim"></param>
        /// <param name="request"></param>
        /// <param name="now"></param>
        private static void UpdateDeviceClaim(DeviceClaim deviceClaim, ClaimRegistrationRequest request, DateTime now)
        {
            deviceClaim.DeviceClaimStatus = DeviceClaimStatus.Approved;
            deviceClaim.ResolvedAt = now;
            deviceClaim.ResolvedByUserId = deviceClaim.MicroCtrlDevice.UserId;
            deviceClaim.ErrorCode = null;
            deviceClaim.ClaimCodeHash = ComputeClaimCodeHash(request.ClaimCode);
            deviceClaim.RequestedAt = now;
            deviceClaim.ExpiresAt = GetClaimExpiration(now, request.ExpiresInSeconds, deviceClaim.ProvisioningTokenExpiresAt);
            deviceClaim.MicroCtrlDevice.ProvisioningState = ProvisioningState.PendingClaim;
            deviceClaim.MicroCtrlDevice.ClaimedAt = now;

            deviceClaim.AttemptCount++;
        }

        private ClaimResponse? ValidateClaimState(ClaimRegistrationRequest request, DeviceClaim deviceClaim)
        {
            string claimCodeHash = ComputeClaimCodeHash(request.ClaimCode);

            if (deviceClaim.DeviceClaimStatus == DeviceClaimStatus.Approved)
                return GetExistingApprovedClaim(deviceClaim, request, claimCodeHash);

            if (deviceClaim.DeviceClaimStatus != DeviceClaimStatus.Pending)
                throw new ConflictException("El claim ya tiene una decisión definitiva.");

            if (deviceClaim.RequestId is not null && deviceClaim.RequestId != request.RequestId)
                throw new ConflictException("El claim ya fue registrado con otro RequestId.");

            return null;
        }

        private ClaimResponse GetExistingApprovedClaim(DeviceClaim deviceClaim, ClaimRegistrationRequest request, string claimCodeHash)
        {
            bool sameRequest =
                deviceClaim.RequestId == request.RequestId &&
                deviceClaim.ContractVersion == request.ContractVersion &&
                FixedTimeEquals(deviceClaim.ClaimCodeHash, claimCodeHash);

            if (!sameRequest)
                throw new ConflictException("El claim fue aprobado con otros datos.");

            if (string.IsNullOrWhiteSpace(deviceClaim.ResponseAuthenticationTag))
                throw new ValidationException("El claim aprobado no contiene una firma.");

            ClaimResponse response = _mapper.Map<ClaimResponse>(deviceClaim);
            response.Validate();

            return response;
        }

        private static void VerifyApprovedClaimResponse(DeviceClaim deviceClaim)
        {
            if (deviceClaim.DeviceClaimStatus != DeviceClaimStatus.Approved)
                throw new ConflictException("El claim todavía no tiene una respuesta aprobada.");

            if (string.IsNullOrWhiteSpace(deviceClaim.ResponseAuthenticationTag))
                throw new ValidationException("El claim aprobado no contiene una firma.");
        }

        private static bool FixedTimeEquals(string? expectedHash, string actualHash)
        {
            if (string.IsNullOrWhiteSpace(expectedHash))
                return false;

            try
            {
                byte[] expected = Convert.FromHexString(expectedHash);
                byte[] actual = Convert.FromHexString(actualHash);

                return expected.Length == actual.Length &&
                       CryptographicOperations.FixedTimeEquals(expected, actual);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
