using AutoMapper;
using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Constants;
using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;
using System.Security.AccessControl;

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

            MicrocontrollerDevice foundMicrocontroller = await _deviceRepository.FindById(device.MicrocontrollerDeviceId, true, ct)
                ?? throw new NotFoundException("Microcontrolador no encontrado.");

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

            Device deviceToSave = _mapper.Map<Device>(device);

            await _deviceRepository.AddDevice(deviceToSave, ct);
            await _deviceRepository.SaveChanges(ct);
            return _mapper.Map<DeviceInventoryGeneralDto>(deviceToSave);
        }

        public async Task<bool> SetDeviceState(int[] deviceIds, bool state, CancellationToken ct)
        {
            IEnumerable<Device> foundDevices = await _deviceRepository.GetDevicesByIds(deviceIds, ct);

            if (!foundDevices.Any())
                return false;

            foreach(Device device in foundDevices)
            {
                device.Active = state;
            }

            await _deviceRepository.SaveChanges(ct);

            return true;
        }

        public async Task<IReadOnlyCollection<DeviceInventoryGeneralDto>> GetAllDevicesByMicrocontrollerId(DeviceInventoryFilters request, CancellationToken ct)
        {
            MicrocontrollerDevice foundMicrocontroller = await _deviceRepository.FindById(request.SelectedMicrocontrollerId, false, ct) 
                ?? throw new NotFoundException("Microcontrolador no encontrado.");

            return _mapper.Map<IReadOnlyCollection<DeviceInventoryGeneralDto>>(await _deviceRepository.GetAllDevicesByMicrocontrollerId(request, ct));
        }

        public async Task<DeviceInventoryGeneralDto?> GetDeviceById(int deviceId, CancellationToken ct)
            => await _deviceRepository.GetDeviceById(deviceId, ct) is Device foundDevice ? _mapper.Map<DeviceInventoryGeneralDto>(foundDevice) 
                : throw new NotFoundException("Dispositivo no encontrado.");

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

        public async Task<DeviceInventoryGeneralDto> UpdateDevice(DeviceInventoryGeneralDto device, CancellationToken ct)
        {
            device.Validate();

            if (!device.Id.HasValue)
                throw new ValidationException("El Id del dispositivo es obligatorio para actualizarlo.");

            Device foundDevice = await _deviceRepository.GetDeviceById(device.Id.Value, ct)
                ?? throw new NotFoundException("Dispositivo no encontrado.");

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

        public async Task<DeviceInventoryGeneralDto?> GetDeviceByLocalId(GetDeviceByLocalIdDto request, CancellationToken ct)
            => await  _deviceRepository.GetDeviceByLocalId(request, ct) is Device foundDevice ? 
                _mapper.Map<DeviceInventoryGeneralDto>(foundDevice) : throw new NotFoundException("Dispositivo no encontrado.");

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
                            CustomName = device.Name,
                            Name = device.Name,
                            DevicePin = device.Esp32Pin,
                            ConnectionType = device.ConnectionType,
                            MeasurementType = device.MeasurementType,
                            MeasurementUnit = device.MeasurementUnit,
                            DeviceType = device.DeviceType,
                            Available = device.Available,
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
                || deviceItem.Available != device.Available)
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
    }
}
