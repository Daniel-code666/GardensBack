using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;
using GardensBack.Application.DeviceInventory.Constants;
using AutoMapper;

namespace GardensBack.Application.DeviceInventory.Repositories
{
    public class DeviceInventoryService(IDeviceInventoryRepository deviceRepository, IDeviceFirmwareUpdateRepository firmwareUpdateRepository,
        IMapper mapper) : IDeviceInventoryService
    {
        private readonly IDeviceInventoryRepository _deviceRepository = deviceRepository;
        private readonly IDeviceFirmwareUpdateRepository _firmwareUpdateRepository = firmwareUpdateRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<DeviceInventoryGeneralDto> AddDevice(DeviceInventoryGeneralDto device, CancellationToken ct)
        {
            device.Validate();

            if (device.DeviceType == DeviceType.Actuator && (device.MeasurementType.HasValue || device.MeasurementUnit.HasValue))
                throw new ValidationException("Dispositivos de tipo actuador no pueden tener tipo o unidad de medición.");

            if (device.MeasurementType.HasValue && !device.MeasurementUnit.HasValue)
                throw new ValidationException("Si se especifica un tipo de medición, también debe especificarse la unidad de medición.");

            if (!device.MeasurementType.HasValue && device.MeasurementUnit.HasValue)
                throw new ValidationException("Si se especifica una unidad de medición, también debe especificarse el tipo de medición.");

            if (device.MeasurementType.HasValue && device.MeasurementUnit.HasValue
                && !Constants.Constants.ValidMeasurements.Contains((device.MeasurementType.Value, device.MeasurementUnit.Value)))
                throw new ConflictException("El tipo y unidad de medición especificados no son válidos.");

            Device deviceToSave = _mapper.Map<Device>(device);

            await _deviceRepository.AddDevice(deviceToSave, ct);
            return device;
        }

        public async Task<DeviceInventoryGeneralDto> ChangeDeviceState(int deviceId, bool state, CancellationToken ct)
        {
            Device foundDevice = await _deviceRepository.GetDeviceById(deviceId, ct) ?? throw new NotFoundException("Dispositivo no encontrado.");

            if (foundDevice.Active == state)
                return _mapper.Map<DeviceInventoryGeneralDto>(foundDevice);

            foundDevice.Active = state;
            await _deviceRepository.SaveChanges(ct);
            return _mapper.Map<DeviceInventoryGeneralDto>(foundDevice);
        }

        public async Task<IReadOnlyCollection<DeviceInventoryGeneralDto>> GetAllDevicesByMicrocontrollerId(int microCtrlId, int[]? devicesIds, CancellationToken ct)
        {
            MicrocontrollerDevice foundMicrocontroller = await _deviceRepository.FindById(microCtrlId, ct) ?? throw new NotFoundException("Microcontrolador no encontrado.");

            return _mapper.Map<IReadOnlyCollection<DeviceInventoryGeneralDto>>(await _deviceRepository.GetAllDevicesByMicrocontrollerId(foundMicrocontroller.Id, devicesIds, ct));
        }

        public async Task<DeviceInventoryGeneralDto?> GetDeviceById(int deviceId, CancellationToken ct)
            => await _deviceRepository.GetDeviceById(deviceId, ct) is Device foundDevice ? _mapper.Map<DeviceInventoryGeneralDto>(foundDevice) : null;

        public async Task ProcessInventory(DeviceInventoryMessage inventory, CancellationToken ct)
        {
            inventory.Validate();

            foreach (DeviceInventoryItemMessage item in inventory.Devices)
                item.Validate();

            MicrocontrollerDevice? foundDevice = await _deviceRepository.FindByDeviceId(inventory.DeviceId, ct);

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

            DeviceFirmwareUpdate? update = await _firmwareUpdateRepository.FindAwaitingInventory(foundDevice.Id, inventory.FirmwareVersion, ct);
            if (update is not null && update.InventoryConfirmedAt is null)
            {
                DateTime now = DateTime.UtcNow;
                update.InventoryConfirmedAt = now;
                changed = true;

            }

            if (changed)
                await _deviceRepository.SaveChanges(ct);

            if (update is not null)
                await _firmwareUpdateRepository.CompleteConfirmedIfReady(update.Id, DateTime.UtcNow, ct);
        }

        public async Task<DeviceInventoryGeneralDto> UpdateDevice(DeviceInventoryGeneralDto device, CancellationToken ct)
        {
            device.Validate();

            if (!device.Id.HasValue)
                throw new ValidationException("El Id del dispositivo es obligatorio para actualizarlo.");

            Device foundDevice = await _deviceRepository.GetDeviceById(device.Id.Value, ct) ?? throw new NotFoundException("Dispositivo no encontrado.");

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

            _mapper.Map<DeviceInventoryGeneralDto, Device>(device, foundDevice);

            await _deviceRepository.SaveChanges(ct);

            return device;
        }
    }
}
