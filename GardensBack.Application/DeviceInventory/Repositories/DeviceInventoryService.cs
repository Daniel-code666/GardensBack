using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Repositories
{
    public class DeviceInventoryService(IDeviceInventoryRepository deviceRepository, IDeviceFirmwareUpdateRepository firmwareUpdateRepository) : IDeviceInventoryService
    {
        private readonly IDeviceInventoryRepository _deviceRepository = deviceRepository;
        private readonly IDeviceFirmwareUpdateRepository _firmwareUpdateRepository = firmwareUpdateRepository;

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
    }
}
