using AutoMapper;
using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.Common.Messaging;
using GardensBack.Application.Common.Security;
using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Constants;
using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;

namespace GardensBack.Application.DeviceInventory.Repositories
{
    public class DeviceInventoryService(IDeviceInventoryRepository deviceRepository,
        IDeviceFirmwareUpdateRepository firmwareUpdateRepository, IDeviceActionPublisher publisher,
        IDeviceCommandKeyProtector commandKeyProtector,
        ICommandAuthenticationService<DeviceActionMessage> authenticationService,
        IDeviceCommandResponseWaiter responseWaiter,
        IUserRepository userRepository,
        IMapper mapper) : IDeviceInventoryService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IDeviceInventoryRepository _deviceRepository = deviceRepository;
        private readonly IDeviceFirmwareUpdateRepository _firmwareUpdateRepository = firmwareUpdateRepository;
        private readonly IMapper _mapper = mapper;
        private readonly IDeviceActionPublisher _publisher = publisher;
        private readonly IDeviceCommandKeyProtector _commandKeyProtector = commandKeyProtector;
        private readonly ICommandAuthenticationService<DeviceActionMessage> _authenticationService = authenticationService;
        private readonly IDeviceCommandResponseWaiter _responseWaiter = responseWaiter;
        private static readonly TimeSpan CommandExpiration = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan ImmediateResponseTimeout = TimeSpan.FromSeconds(10);
        private static readonly JsonSerializerOptions CommandJsonOptions = CreateCommandJsonOptions();

        public async Task<DeviceInventoryGeneralDto> AddDevice(DeviceInventoryGeneralDto device, int requestedByUserId, CancellationToken ct)
        {
            device.Validate();
            ValidateRequestedByUserId(requestedByUserId);

            MicrocontrollerDevice foundMicrocontroller = await _deviceRepository.FindById(device.MicrocontrollerDeviceId, true, ct)
                ?? throw new NotFoundException("Microcontrolador no encontrado.");

            ValidateDeviceOwnership(foundMicrocontroller, requestedByUserId);

            bool hasAnyI2cValue = device.I2cSdaPin.HasValue || device.I2cSclPin.HasValue || device.I2cAddress.HasValue;

            bool hasCompleteI2cValue = device.I2cSdaPin.HasValue && device.I2cSclPin.HasValue && device.I2cAddress.HasValue;

            if (device.DeviceType == DeviceType.Actuator && (device.MeasurementType.HasValue || device.MeasurementUnit.HasValue))
                throw new ValidationException("Dispositivos de tipo actuador no pueden tener tipo o unidad de medición.");

            if (device.MeasurementType.HasValue && !device.MeasurementUnit.HasValue)
                throw new ValidationException("Si se especifica un tipo de medición, también debe especificarse la unidad de medición.");

            if (!device.MeasurementType.HasValue && device.MeasurementUnit.HasValue)
                throw new ValidationException("Si se especifica una unidad de medición, también debe especificarse el tipo de medición.");

            if (device.MeasurementType.HasValue && device.MeasurementUnit.HasValue
                && !Constants.Constants.ValidMeasurements.Contains((device.MeasurementType.Value, device.MeasurementUnit.Value)))
                throw new ConflictException("El tipo y unidad de medición especificados no son válidos.");

            if (foundMicrocontroller.Devices.Any(x => x.LocalDeviceId == device.LocalDeviceId))
                throw new ConflictException("Ya existe un dispositivo con el mismo Id local en este microcontrolador.");

            if (foundMicrocontroller.Devices.Any(x => x.DevicePin == device.DevicePin))
                throw new ConflictException("Ya existe un dispositivo con el mismo pin en este microcontrolador.");

            if (foundMicrocontroller.Devices.Any(x => x.Name == device.Name))
                throw new ConflictException("Ya existe un dispositivo con el mismo nombre en este microcontrolador.");

            if (hasAnyI2cValue && !hasCompleteI2cValue)
                throw new ValidationException("La configuración I2C debe incluir SDA, SCL y dirección.");

            if (hasAnyI2cValue && device.ConnectionType != ConnectionType.I2C)
                throw new ValidationException("La configuración I2C solo es válida para conexiones I2C.");

            Device deviceToSave = _mapper.Map<Device>(device);

            await _deviceRepository.AddDevice(deviceToSave, ct);
            await _deviceRepository.SaveChanges(ct);
            return _mapper.Map<DeviceInventoryGeneralDto>(deviceToSave);
        }

        public async Task<bool> SetDeviceState(int[] deviceIds, bool state, int requestedByUserId, CancellationToken ct)
        {
            ValidateRequestedByUserId(requestedByUserId);

            if (deviceIds is null || deviceIds.Length == 0 || deviceIds.Any(id => id <= 0))
                throw new ValidationException("Debe especificar al menos un dispositivo válido.");

            int[] requestedDeviceIds = [.. deviceIds.Distinct()];
            List<Device> foundDevices = [.. await _deviceRepository.GetDevicesByIds(requestedDeviceIds, ct)];

            if (foundDevices.Count != requestedDeviceIds.Length)
                throw new NotFoundException("Uno o más dispositivos no fueron encontrados.");

            if (foundDevices.Any(device => device.MicroCtrlDevice.UserId != requestedByUserId))
                throw new ForbiddenException("Uno o más dispositivos no pertenecen al usuario autenticado.");

            foreach(Device device in foundDevices)
            {
                device.Active = state;
            }

            await _deviceRepository.SaveChanges(ct);

            return true;
        }

        public async Task<IReadOnlyCollection<DeviceInventoryGeneralDto>> GetAllDevicesByMicrocontrollerId(DeviceInventoryFilters request, int requestedByUserId, CancellationToken ct)
        {
            ValidateRequestedByUserId(requestedByUserId);

            if (request.TelemtryPerRequest < 1)
                throw new ValidationException("La cantidad de registros de telemetría debe ser mayor que cero.");

            ValidateTelemetryDateRange(request.StartDateUtc, request.EndDateUtc);

            MicrocontrollerDevice foundMicrocontroller = await _deviceRepository.FindById(request.SelectedMicrocontrollerId, false, ct) 
                ?? throw new NotFoundException("Microcontrolador no encontrado.");

            ValidateDeviceOwnership(foundMicrocontroller, requestedByUserId);

            return _mapper.Map<IReadOnlyCollection<DeviceInventoryGeneralDto>>(await _deviceRepository.GetAllDevicesByMicrocontrollerId(request, ct));
        }

        public async Task<IReadOnlyList<MicroControllerDeviceDto>> GetAllMicrocontrollerDevicesAssociatedToUser(int userId, CancellationToken ct)
        {
            User foundUser = await _userRepository.GetUserWithMicroDevices(userId, true, ct) ?? throw new NotFoundException("Usuario no encontrado");

            return _mapper.Map<IReadOnlyList<MicroControllerDeviceDto>>(foundUser.MicrocontrollerDevices);
        }

        public async Task<MicrocontrollerWDevicesDto> GetMicroControllerDeviceById(int id, int requestedByUserId, CancellationToken ct)
        {
            MicrocontrollerDevice? foundMicrocontroller = await _deviceRepository.GetMicroControllerDeviceById(id, ct) 
                ?? throw new NotFoundException("Microcontrolador no encontrado");

            ValidateDeviceOwnership(foundMicrocontroller, requestedByUserId);

            return _mapper.Map<MicrocontrollerWDevicesDto>(foundMicrocontroller);
        }

        public async Task<IReadOnlyList<DeviceTelemetryReadingDto>> GetTelemetryByDeviceId(DeviceTelemetryRequest request, int userId, CancellationToken ct)
        {
            ValidateRequestedByUserId(userId);

            if (request.ItemPerRequest < 1)
                throw new ValidationException("La cantidad de registros de telemetría debe ser mayor que cero.");

            (DateTime? startDateUtc, DateTime? endDateUtc) = ValidateTelemetryDateRange(
                request.StartDateUtc,
                request.EndDateUtc);

            if (!await _deviceRepository.CheckMicroCtrlDeviceUserOwnership(request.DeviceId, request.MicrocontrollerId, userId, ct))
                throw new ValidationException("Los datos de pertenencia del usuario, microcontrolador y dispositivo del microcontrolador no coinciden");

            return _mapper.Map<IReadOnlyList<DeviceTelemetryReadingDto>>(await _deviceRepository.GetTelemetryByDeviceId(
                request.DeviceId,
                request.ItemPerRequest,
                startDateUtc,
                endDateUtc,
                ct));
        }

        public async Task<DeviceInventoryGeneralDto?> GetDeviceById(int deviceId, int requestedByUserId, CancellationToken ct)
        {
            ValidateRequestedByUserId(requestedByUserId);

            Device foundDevice = await _deviceRepository.GetDeviceById(deviceId, ct)
                ?? throw new NotFoundException("Dispositivo no encontrado.");

            ValidateDeviceOwnership(foundDevice.MicroCtrlDevice, requestedByUserId);
            return _mapper.Map<DeviceInventoryGeneralDto>(foundDevice);
        }

        public async Task ProcessInventory(DeviceInventoryMessage inventory, CancellationToken ct)
        {
            inventory.Validate();

            foreach (DeviceInventoryItemMessage item in inventory.Devices)
                item.Validate();

            MicrocontrollerDevice? foundDevice = await _deviceRepository.FindByDeviceId(inventory.DeviceId, true, ct);

            if (foundDevice is null || !foundDevice.Active)
                return;

            if (foundDevice.ProvisioningState != ProvisioningState.Provisioned)
                return;

            bool changed = false;

            if (!string.Equals(foundDevice.FirmwareVersion, inventory.FirmwareVersion, StringComparison.Ordinal))
            {
                foundDevice.FirmwareVersion = inventory.FirmwareVersion;
                changed = true;
            }

            // actualizar los dispositivos reportados por el microcontrolador
            bool devicesChanged = UpdateMicrocontrollerDevices(foundDevice, inventory.Devices);

            DeviceFirmwareUpdate? update = await _firmwareUpdateRepository.FindAwaitingInventory(foundDevice.Id, inventory.FirmwareVersion, ct);

            if (update is not null && update.InventoryConfirmedAt is null)
            {
                DateTime now = DateTime.UtcNow;
                update.InventoryConfirmedAt = now;
                changed = true;
            }

            if (changed || devicesChanged)
                await _deviceRepository.SaveChanges(ct);

            if (update is not null)
                await _firmwareUpdateRepository.CompleteConfirmedIfReady(update.Id, DateTime.UtcNow, ct);
        }

        public async Task<DeviceInventoryGeneralDto> UpdateDevice(DeviceInventoryGeneralDto device, int requestedByUserId, CancellationToken ct)
        {
            device.Validate();
            ValidateRequestedByUserId(requestedByUserId);

            if (!device.Id.HasValue)
                throw new ValidationException("El Id del dispositivo es obligatorio para actualizarlo.");

            Device foundDevice = await _deviceRepository.GetDeviceById(device.Id.Value, ct)
                ?? throw new NotFoundException("Dispositivo no encontrado.");

            ValidateDeviceOwnership(foundDevice.MicroCtrlDevice, requestedByUserId);

            if (device.DeviceType == DeviceType.Actuator && (device.MeasurementType.HasValue || device.MeasurementUnit.HasValue))
                throw new ValidationException("Dispositivos de tipo actuador no pueden tener tipo o unidad de medición.");

            if (device.MeasurementType.HasValue && !device.MeasurementUnit.HasValue)
                throw new ValidationException("Si se especifica un tipo de medición, también debe especificarse la unidad de medición.");

            if (!device.MeasurementType.HasValue && device.MeasurementUnit.HasValue)
                throw new ValidationException("Si se especifica una unidad de medición, también debe especificarse el tipo de medición.");

            if (device.MeasurementType.HasValue && device.MeasurementUnit.HasValue
                && !Constants.Constants.ValidMeasurements.Contains((device.MeasurementType.Value, device.MeasurementUnit.Value)))
                throw new ConflictException("El tipo y unidad de medición especificados no son válidos.");

            if (device.DeviceType != foundDevice.DeviceType)
                throw new ValidationException("No se puede cambiar el tipo de dispositivo.");

            if (device.DevicePin != foundDevice.DevicePin)
                throw new ValidationException("No se puede cambiar el pin del dispositivo.");

            string? customName = string.IsNullOrWhiteSpace(device.CustomName) ? null : device.CustomName.Trim();

            device.CustomName = customName;
            bool changed = CheckIfDeviceIsChangedFromManualUpdate(foundDevice, device);

            if (!changed)
                return _mapper.Map<DeviceInventoryGeneralDto>(foundDevice);

            SetDeviceUpdateProperties(foundDevice, device);

            await _deviceRepository.SaveChanges(ct);

            return _mapper.Map<DeviceInventoryGeneralDto>(foundDevice);
        }

        public async Task<DeviceInventoryGeneralDto?> GetDeviceByLocalId(
            GetDeviceByLocalIdDto request,
            int requestedByUserId,
            CancellationToken ct)
        {
            ValidateRequestedByUserId(requestedByUserId);

            Device foundDevice = await _deviceRepository.GetDeviceByLocalId(request, ct)
                ?? throw new NotFoundException("Dispositivo no encontrado.");

            ValidateDeviceOwnership(foundDevice.MicroCtrlDevice, requestedByUserId);
            return _mapper.Map<DeviceInventoryGeneralDto>(foundDevice);
        }

        public async Task ProcessTelemetry(string microcontrollerDeviceId, DeviceTelemetryMessage telemetry, CancellationToken ct)
        {
            bool telemetryAdded = await AddTelemetry(
                microcontrollerDeviceId,
                telemetry,
                TelemetrySource.PeriodicTelemetry,
                requestId: null,
                ct);

            if (telemetryAdded)
                await _deviceRepository.SaveChanges(ct);
        }

        public async Task<IEnumerable<DeviceTelemetryDto>> GetTelemetryForDevices(
            GetTelemetryForDevicesDto devicesDto,
            int requestedByUserId,
            CancellationToken ct)
        {
            ValidateRequestedByUserId(requestedByUserId);

            if (devicesDto.DevicesIds is null || devicesDto.DevicesIds.Length == 0 ||
                devicesDto.DevicesIds.Any(id => id <= 0))
                throw new ValidationException("Debe especificar al menos un dispositivo válido.");

            if (devicesDto.ItemPerRequest < 1)
                throw new ValidationException("La cantidad de registros de telemetría debe ser mayor que cero.");

            (DateTime? startDateUtc, DateTime? endDateUtc) = ValidateTelemetryDateRange(
                devicesDto.StartDateUtc,
                devicesDto.EndDateUtc);

            int[] requestedDeviceIds = [.. devicesDto.DevicesIds.Distinct()];
            List<Device> foundDevices = [.. await _deviceRepository.GetDevicesWithTelemetry(
                requestedDeviceIds,
                devicesDto.ItemPerRequest,
                startDateUtc,
                endDateUtc,
                ct)];

            if (foundDevices.Count != requestedDeviceIds.Length)
                throw new NotFoundException("Uno o más dispositivos no fueron encontrados.");

            if (foundDevices.Any(device => device.MicroCtrlDevice.UserId != requestedByUserId))
                throw new ForbiddenException("Uno o más dispositivos no pertenecen al usuario autenticado.");

            return _mapper.Map<IEnumerable<DeviceTelemetryDto>>(foundDevices);
        }

        public async Task<DeviceCommandExecutionResponseDto> CreateDeviceCommand(DeviceCommandRequestDto request, int requestedByUserId, bool waitForResponse, CancellationToken ct)
        {
            request.Validate();

            ValidateRequestedByUserId(requestedByUserId);

            int[] requestedDeviceIds = [.. request.DeviceIds.Distinct()];
            List<Device> devices = [.. await _deviceRepository.GetDevicesByIds(requestedDeviceIds, ct)];

            if (devices.Any(x => string.IsNullOrEmpty(x.MicroCtrlDevice.DeviceId)))
                throw new ValidationException("El microntrolador no tiene un deviceId");

            if (devices.Count != requestedDeviceIds.Length)
                throw new NotFoundException("Uno o más dispositivos no fueron encontrados.");

            if (devices.Any(device => !device.Active))
                throw new ValidationException("Todos los dispositivos seleccionados deben estar activos.");

            if (devices.Select(device => device.MicroCtrlDeviceId).Distinct().Count() != 1)
                throw new ValidationException("Todos los dispositivos deben pertenecer al mismo microcontrolador.");

            MicrocontrollerDevice microcontroller = devices[0].MicroCtrlDevice;
            ValidateMicrocontrollerForCommand(microcontroller);
            ValidateDeviceOwnership(microcontroller, requestedByUserId);

            if (request.Command != DeviceCommandType.GetSensorOrActuatorDataById &&
                devices.Any(device => device.DeviceType != DeviceType.Actuator))
                throw new ValidationException("Los comandos de control solo pueden enviarse a actuadores.");

            string protectedCommandKey = microcontroller.MqttCredential?.ProtectedCommandKey
                ?? throw new ValidationException("El dispositivo no tiene registrada la clave necesaria para autenticar comandos remotos.");
            byte[] commandKey = _commandKeyProtector.Unprotect(protectedCommandKey);

            try
            {
                uint lastCommandVersion = await _deviceRepository.GetLastCommandVersion(microcontroller.Id, ct);
                if (lastCommandVersion == uint.MaxValue)
                    throw new ConflictException("La secuencia de comandos alcanzó el valor máximo permitido.");

                DateTime now = DateTime.UtcNow;
                long unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
                long expiresAtUnixSeconds = new DateTimeOffset(now.Add(CommandExpiration)).ToUnixTimeSeconds();
                if (unixTimeSeconds <= 0 || expiresAtUnixSeconds > uint.MaxValue)
                    throw new ConflictException("No fue posible generar una versión o expiración válida para el comando.");

                uint commandVersion = Math.Max(lastCommandVersion + 1, (uint)unixTimeSeconds);
                string requestId = Guid.NewGuid().ToString("N");
                DeviceActionMessage message = BuildActionMessage(
                    microcontroller.DeviceId!,
                    requestId,
                    commandVersion,
                    (uint)expiresAtUnixSeconds,
                    request.Command,
                    BuildCommandParameters(devices, request));

                message.AuthenticationTag = _authenticationService.Sign(message, commandKey);
                Task<DeviceActionResultMessage?>? responseTask = waitForResponse ? _responseWaiter.WaitAsync(requestId, ImmediateResponseTimeout, ct) : null;

                try
                {
                    var command = new DeviceCommand
                    {
                        Command = request.Command.ToString(),
                        RequestId = requestId,
                        CommandVersion = commandVersion,
                        Payload = JsonSerializer.Serialize(message, CommandJsonOptions),
                        Status = CommandStatus.Pending,
                        ExpiresAt = now.Add(CommandExpiration),
                        MicroCtrlDeviceId = microcontroller.Id,
                        MicroCtrlDevice = microcontroller
                    };

                    await _deviceRepository.AddCommand(command, ct);
                    await _deviceRepository.SaveChanges(ct);
                    await PublishActionCommandMessage(command, message, ct);

                    DeviceActionResultMessage? deviceResponse = responseTask is null ? null : await responseTask;

                    if (deviceResponse is not null)
                        await _deviceRepository.ReloadCommand(command, ct);

                    return new DeviceCommandExecutionResponseDto
                    {
                        Command = _mapper.Map<DeviceCommandResponseDto>(command),
                        DeviceResponse = deviceResponse
                    };
                }
                catch
                {
                    if (responseTask is not null)
                        _responseWaiter.Cancel(requestId);

                    throw;
                }
            }
            finally
            {
                CryptographicOperations.ZeroMemory(commandKey);
            }
        }

        public async Task<DeviceCommandResponseDto> GetDeviceCommandById(int commandId, int requestedByUserId, CancellationToken ct)
        {
            ValidateRequestedByUserId(requestedByUserId);

            if (commandId <= 0)
                throw new ValidationException("El identificador del comando no es válido.");

            DeviceCommand command = await _deviceRepository.GetCommandById(commandId, ct)
                ?? throw new NotFoundException("El comando no fue encontrado.");

            ValidateDeviceOwnership(command.MicroCtrlDevice, requestedByUserId);

            await ExpireIfNecessary(command, ct);
            return _mapper.Map<DeviceCommandResponseDto>(command);
        }

        public async Task<DeviceCommandResponseDto> GetDeviceCommandByRequest(string deviceId, string requestId, int requestedByUserId, CancellationToken ct)
        {
            ValidateRequestedByUserId(requestedByUserId);

            if (!DeviceMqttTopics.IsValidDeviceId(deviceId))
                throw new ValidationException("El identificador del microcontrolador no es válido.");

            if (string.IsNullOrWhiteSpace(requestId) || requestId.Length > 64)
                throw new ValidationException("El identificador de la solicitud no es válido.");

            DeviceCommand command = await _deviceRepository.GetCommandByRequest(deviceId, requestId, ct)
                ?? throw new NotFoundException("El comando no fue encontrado.");

            ValidateDeviceOwnership(command.MicroCtrlDevice, requestedByUserId);

            await ExpireIfNecessary(command, ct);
            return _mapper.Map<DeviceCommandResponseDto>(command);
        }

        public async Task<DeviceCommandResponseDto> MarkDeviceCommandPublished(int commandId, CancellationToken ct)
        {
            DeviceCommand command = await GetCommand(commandId, ct);

            if (command.Status is CommandStatus.Completed or CommandStatus.Failed or CommandStatus.Expired)
                return _mapper.Map<DeviceCommandResponseDto>(command);

            if (command.Status == CommandStatus.Published)
                return _mapper.Map<DeviceCommandResponseDto>(command);

            DateTime now = DateTime.UtcNow;

            if (command.ExpiresAt <= now)
            {
                SetExpired(command, now);
                await _deviceRepository.SaveChanges(ct);
            }
            else
            {
                int updated = await _deviceRepository.MarkCommandPublished(command.Id, now, ct);
                if (updated == 1)
                {
                    command.Status = CommandStatus.Published;
                    command.PublishedAt = now;
                    command.ErrorCode = null;
                    command.ErrorMessage = null;
                }
                else
                {
                    await _deviceRepository.ReloadCommand(command, ct);
                }
            }

            return _mapper.Map<DeviceCommandResponseDto>(command);
        }

        public async Task<DeviceCommandResponseDto> MarkDeviceCommandPublishFailed(
            int commandId,
            string errorCode,
            string errorMessage,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(errorCode) || string.IsNullOrWhiteSpace(errorMessage))
                throw new ValidationException("El código y el mensaje del error son obligatorios.");

            DeviceCommand command = await GetCommand(commandId, ct);

            if (command.Status is CommandStatus.Completed or CommandStatus.Failed or CommandStatus.Expired)
                return _mapper.Map<DeviceCommandResponseDto>(command);

            if (command.Status != CommandStatus.Pending)
                throw new ConflictException("El comando ya fue publicado y no puede marcarse como un fallo de publicación.");

            command.Status = CommandStatus.Failed;
            command.Success = false;
            command.ErrorCode = errorCode.Trim();
            command.ErrorMessage = errorMessage.Trim();
            command.CompletedAt = DateTime.UtcNow;

            await _deviceRepository.SaveChanges(ct);
            return _mapper.Map<DeviceCommandResponseDto>(command);
        }

        public async Task ProcessDeviceCommandResponse(string deviceId, DeviceActionResultMessage response, CancellationToken ct)
        {
            response.Validate();

            if (!string.Equals(deviceId, response.DeviceId, StringComparison.Ordinal))
                throw new ValidationException("El microcontrolador del tópico no coincide con el de la respuesta.");

            DeviceCommand command = await _deviceRepository.GetCommandByRequest(deviceId, response.RequestId, ct)
                ?? throw new NotFoundException("No existe un comando asociado a la respuesta recibida.");

            if (command.Status is CommandStatus.Completed or CommandStatus.Failed or CommandStatus.Expired)
                return;

            if (!string.Equals(command.Command, response.Command.ToString(), StringComparison.Ordinal))
                throw new ValidationException("El comando de la respuesta no coincide con la solicitud registrada.");

            DateTime now = DateTime.UtcNow;
            if (command.ExpiresAt <= now)
            {
                SetExpired(command, now);
                await _deviceRepository.SaveChanges(ct);
                return;
            }

            command.ResponsePayload = JsonSerializer.Serialize(response, CommandJsonOptions);
            command.Success = response.Success;
            command.Status = response.Success ? CommandStatus.Completed : CommandStatus.Failed;
            command.CompletedAt = now;
            command.ErrorCode = response.Success ? null : "device_action_failed";
            command.ErrorMessage = response.Success ? null : "El dispositivo informó que no pudo ejecutar el comando.";

            await AddTelemetry(
                deviceId,
                response.Telemetry,
                TelemetrySource.CommandResponse,
                response.RequestId,
                ct);

            await _deviceRepository.SaveChanges(ct);
            _responseWaiter.Complete(response.RequestId, response);
        }

        public Task<int> ExpireDeviceCommands(DateTime utcNow, CancellationToken ct)
        {
            if (utcNow.Kind != DateTimeKind.Utc)
                throw new ValidationException("La fecha de expiración debe estar expresada en UTC.");

            return _deviceRepository.ExpireCommands(utcNow, ct);
        }

        private async Task<bool> AddTelemetry(
            string microcontrollerDeviceId,
            DeviceTelemetryMessage telemetry,
            TelemetrySource source,
            string? requestId,
            CancellationToken ct)
        {
            if (telemetry.SensorReadings.Count == 0 && telemetry.ActuatorReadings.Count == 0)
                return false;

            MicrocontrollerDevice? microcontroller = await _deviceRepository.FindByDeviceId(microcontrollerDeviceId, true, ct);

            if (microcontroller is null || microcontroller.Devices.Count == 0)
                return false;

            bool telemetryAdded = false;

            foreach (SensorReadingMessage sensorTelemetry in telemetry.SensorReadings)
            {
                Device? foundDevice = microcontroller.Devices.SingleOrDefault(x =>
                    x.LocalDeviceId == sensorTelemetry.Id &&
                    x.MeasurementType == sensorTelemetry.MeasurementType &&
                    x.MeasurementUnit == sensorTelemetry.MeasurementUnit);

                if (foundDevice is null)
                    continue;

                DeviceTelemetry deviceTelemetry = _mapper.Map<SensorReadingMessage, DeviceTelemetry>(sensorTelemetry);
                deviceTelemetry.ActuatorMode = null;
                deviceTelemetry.State = null;
                deviceTelemetry.DeviceId = foundDevice.Id;
                deviceTelemetry.Source = source;
                deviceTelemetry.RequestId = requestId;

                await _deviceRepository.AddTelemetry(deviceTelemetry, ct);
                telemetryAdded = true;
            }

            foreach (ActuatorReadingMessage actuatorReading in telemetry.ActuatorReadings)
            {
                Device? foundDevice = microcontroller.Devices.SingleOrDefault(x =>
                    x.LocalDeviceId == actuatorReading.Id &&
                    x.ConnectionType == actuatorReading.ConnType &&
                    x.Name == actuatorReading.Name);

                if (foundDevice is null)
                    continue;

                DeviceTelemetry deviceTelemetry = _mapper.Map<ActuatorReadingMessage, DeviceTelemetry>(actuatorReading);
                deviceTelemetry.Value = 0;
                deviceTelemetry.DeviceId = foundDevice.Id;
                deviceTelemetry.Source = source;
                deviceTelemetry.RequestId = requestId;

                await _deviceRepository.AddTelemetry(deviceTelemetry, ct);
                telemetryAdded = true;
            }

            return telemetryAdded;
        }

        private async Task<DeviceCommand> GetCommand(int commandId, CancellationToken ct)
        {
            if (commandId <= 0)
                throw new ValidationException("El identificador del comando no es válido.");

            DeviceCommand command = await _deviceRepository.GetCommandById(commandId, ct)
                ?? throw new NotFoundException("El comando no fue encontrado.");

            return command;
        }

        private async Task ExpireIfNecessary(DeviceCommand command, CancellationToken ct)
        {
            if (command.Status is not (CommandStatus.Pending or CommandStatus.Published) ||
                command.ExpiresAt > DateTime.UtcNow)
                return;

            SetExpired(command, DateTime.UtcNow);
            await _deviceRepository.SaveChanges(ct);
        }

        private static void SetExpired(DeviceCommand command, DateTime utcNow)
        {
            command.Status = CommandStatus.Expired;
            // No recibir la respuesta no demuestra que el dispositivo no ejecutó el comando.
            command.Success = null;
            command.ErrorCode = "command_response_timeout";
            command.ErrorMessage = "No se recibió confirmación del dispositivo antes de la fecha de expiración.";
            command.CompletedAt = utcNow;
        }

        private static void ValidateMicrocontrollerForCommand(MicrocontrollerDevice microcontroller)
        {
            if (!microcontroller.Active)
                throw new ValidationException("El microcontrolador no está activo.");

            if (microcontroller.ProvisioningState != ProvisioningState.Provisioned)
                throw new ValidationException("El microcontrolador no está provisionado.");

            if (!DeviceMqttTopics.IsValidDeviceId(microcontroller.DeviceId))
                throw new ValidationException("El microcontrolador no tiene un identificador MQTT válido.");
        }

        private static void ValidateRequestedByUserId(int requestedByUserId)
        {
            if (requestedByUserId <= 0)
                throw new UnathorizedException("La identidad del usuario no es válida.");
        }

        private static void ValidateDeviceOwnership(MicrocontrollerDevice microcontroller, int requestedByUserId)
        {
            if (microcontroller.UserId != requestedByUserId)
                throw new ForbiddenException("El microcontrolador no pertenece al usuario autenticado.");
        }

        private static (DateTime? StartDateUtc, DateTime? EndDateUtc) ValidateTelemetryDateRange(
            DateTimeOffset? startDateUtc,
            DateTimeOffset? endDateUtc)
        {
            if (startDateUtc.HasValue && endDateUtc.HasValue && startDateUtc.Value > endDateUtc.Value)
                throw new ValidationException("La fecha inicial de telemetría no puede ser posterior a la fecha final.");

            return (startDateUtc?.UtcDateTime, endDateUtc?.UtcDateTime);
        }

        private static IReadOnlyCollection<DeviceActionParameter> BuildCommandParameters(
            IEnumerable<Device> devices,
            DeviceCommandRequestDto request)
        {
            int? modeValue = request.Mode switch
            {
                ActuatorMode.Manual => 1,
                ActuatorMode.Automatic => 2,
                null => null,
                _ => throw new ValidationException("El modo de control solicitado no es válido.")
            };

            return devices
                .GroupBy(device => new { device.LocalDeviceId, device.DeviceType })
                .Select(group => group.First())
                .Select(device => new DeviceActionParameter
                {
                    DeviceId = device.LocalDeviceId,
                    DeviceType = device.DeviceType switch
                    {
                        DeviceType.Sensor => "sensor",
                        DeviceType.Actuator => "actuator",
                        _ => throw new ValidationException("El tipo de dispositivo no admite comandos.")
                    },
                    State = request.Command == DeviceCommandType.SetWaterPumpState ? request.State : null,
                    Value = request.Command == DeviceCommandType.SetWaterPumpControlMode ? modeValue : null
                })
                .ToArray();
        }

        private static JsonSerializerOptions CreateCommandJsonOptions()
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        private static bool UpdateMicrocontrollerDevices(MicrocontrollerDevice microcontroller, IEnumerable<DeviceInventoryItemMessage> devices)
        {
            DateTime now = DateTime.UtcNow;

            if (microcontroller.Devices.Count == 0)
            {
                microcontroller.Devices = devices.Select(deviceItem => new Device
                {
                    LocalDeviceId = deviceItem.LocalDeviceId,
                    Name = deviceItem.Name,
                    DevicePin = deviceItem.Esp32Pin,
                    ConnectionType = deviceItem.ConnectionType,
                    MeasurementType = deviceItem.MeasurementType,
                    MeasurementUnit = deviceItem.MeasurementUnit,
                    DeviceType = deviceItem.DeviceType,
                    Available = deviceItem.Available,
                    I2cSdaPin = deviceItem.I2cSdaPin,
                    I2cSclPin = deviceItem.I2cSclPin,
                    I2cAddress = deviceItem.I2cAddress,
                    LastSeenAt = now,
                    CreationDate = now,
                    UpdateDate = now,
                    MicroCtrlDevice = microcontroller
                }).ToHashSet();

                return true;
            }
            else if (microcontroller.Devices.Count >= 1)
            {
                bool devicesChanged = false;

                // recorrer los dispositivos desde inventory.devices
                // ubicar los dispositivos que coincidan en ambas listas, la de microcontrolador y la de inventory.devices, y actualizar sus propiedades
                foreach (DeviceInventoryItemMessage device in devices)
                {
                    Device? foundDevice = microcontroller.Devices.SingleOrDefault(x => x.LocalDeviceId == device.LocalDeviceId && x.MeasurementType == device.MeasurementType
                        && x.DeviceType == device.DeviceType);

                    // evaluar si foundDevice es null, si es null entonces se agrega
                    if (foundDevice == null)
                    {
                        microcontroller.Devices.Add(new Device
                        {
                            LocalDeviceId = device.LocalDeviceId,
                            Name = device.Name,
                            DevicePin = device.Esp32Pin,
                            ConnectionType = device.ConnectionType,
                            MeasurementType = device.MeasurementType,
                            MeasurementUnit = device.MeasurementUnit,
                            DeviceType = device.DeviceType,
                            Available = device.Available,
                            I2cSdaPin = device.I2cSdaPin,
                            I2cSclPin = device.I2cSclPin,
                            I2cAddress = device.I2cAddress,
                            LastSeenAt = now,
                            CreationDate = now,
                            UpdateDate = now,
                            MicroCtrlDevice = microcontroller
                        });
                        devicesChanged = true;
                    }
                    else if (CheckIfDeviceIsChanged(foundDevice, device)) // si existe y hay diferencias entonces se actualizan
                    {
                        foundDevice.Name = device.Name;
                        foundDevice.DevicePin = device.Esp32Pin;
                        foundDevice.ConnectionType = device.ConnectionType;
                        foundDevice.MeasurementType = device.MeasurementType;
                        foundDevice.MeasurementUnit = device.MeasurementUnit;
                        foundDevice.DeviceType = device.DeviceType;
                        foundDevice.Available = device.Available;
                        foundDevice.I2cSdaPin = device.I2cSdaPin;
                        foundDevice.I2cSclPin = device.I2cSclPin;
                        foundDevice.I2cAddress = device.I2cAddress;
                        foundDevice.LastSeenAt = now;
                        foundDevice.UpdateDate = now;
                        devicesChanged = true;
                    }
                }
                return devicesChanged;
            }
            return false;
        }

        private static bool CheckIfDeviceIsChanged(Device device, DeviceInventoryItemMessage deviceItem)
        {
            if (deviceItem.Name != device.Name || deviceItem.Esp32Pin != device.DevicePin
                || deviceItem.ConnectionType != device.ConnectionType || deviceItem.MeasurementUnit != device.MeasurementUnit
                || deviceItem.Available != device.Available || deviceItem.I2cSdaPin != device.I2cSdaPin || deviceItem.I2cSclPin != device.I2cSclPin
                || deviceItem.I2cAddress != device.I2cAddress)
                return true;

            return false;
        }

        private static bool CheckIfDeviceIsChangedFromManualUpdate(Device device, DeviceInventoryGeneralDto update)
        {
            return device.LocalDeviceId != update.LocalDeviceId || !string.Equals(device.CustomName, update.CustomName, StringComparison.Ordinal) 
                || device.ConnectionType != update.ConnectionType || device.MeasurementType != update.MeasurementType || device.MeasurementUnit != update.MeasurementUnit;
        }

        private static void SetDeviceUpdateProperties(Device device, DeviceInventoryGeneralDto update)
        {
            device.LocalDeviceId = update.LocalDeviceId;
            device.CustomName = update.CustomName;
            device.ConnectionType = update.ConnectionType;
            device.MeasurementType = update.MeasurementType;
            device.MeasurementUnit = update.MeasurementUnit;
            device.UpdateDate = DateTime.UtcNow;
        }

        private async Task PublishActionCommandMessage(DeviceCommand command, DeviceActionMessage message, CancellationToken ct)
        {
            try
            {
                await _publisher.PublishCommand(command.MicroCtrlDevice.DeviceId!, message, ct);
                DateTime publishedAt = DateTime.UtcNow;
                int updated = await _deviceRepository.MarkCommandPublished(command.Id, publishedAt, ct);
                if (updated == 1)
                {
                    command.Status = CommandStatus.Published;
                    command.PublishedAt = publishedAt;
                }
                else
                {
                    await _deviceRepository.ReloadCommand(command, ct);
                }
            }
            catch(OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                command.Status = CommandStatus.Failed;
                command.Success = false;
                command.CompletedAt = DateTime.UtcNow;
                command.ErrorCode = "mqtt_publish_failed";
                command.ErrorMessage = ex.ToString();
                await _deviceRepository.SaveChanges(CancellationToken.None);

                throw;
            }
        }

        private static DeviceActionMessage BuildActionMessage(
            string deviceId,
            string requestId,
            uint commandVersion,
            uint expiresAt,
            DeviceCommandType command,
            IReadOnlyCollection<DeviceActionParameter> messageParams)
            => new()
            {
                DeviceId = deviceId,
                RequestId = requestId,
                CommandVersion = commandVersion,
                ExpiresAt = expiresAt,
                Command = command,
                Params = messageParams,
                AuthenticationTag = string.Empty
            };
    }
}
