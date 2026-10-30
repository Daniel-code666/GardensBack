using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Repositories
{
    public class DeviceInventoryService(IDeviceInventoryRepository deviceRepository) : IDeviceInventoryService
    {
        private readonly IDeviceInventoryRepository _deviceRepository = deviceRepository;

        public async Task ProcessInventory(DeviceInventoryMessage inventory, CancellationToken ct)
        {
            inventory.Validate();

            foreach (DeviceInventoryItemMessage item in inventory.Devices)
                item.Validate();

            MicrocontrollerDevice? foundDevice = await _deviceRepository.FindByDeviceId(inventory.DeviceId, ct);

            if (foundDevice is null || !foundDevice.Active)
                return;

            if (foundDevice.FirmwareVersion == inventory.FirmwareVersion)
                return;

            if (foundDevice.ProvisioningState != ProvisioningState.Provisioned)
                return;

            foundDevice.FirmwareVersion = inventory.FirmwareVersion;

            await _deviceRepository.SaveChanges(ct);
        }
    }
}
