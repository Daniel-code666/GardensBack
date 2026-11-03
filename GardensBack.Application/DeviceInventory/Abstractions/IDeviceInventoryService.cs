using GardensBack.Application.DeviceInventory.Dtos;

namespace GardensBack.Application.DeviceInventory.Abstractions
{
    public interface IDeviceInventoryService
    {
        Task ProcessInventory(DeviceInventoryMessage inventory, CancellationToken ct);
        Task<DeviceInventoryGeneralDto> AddDevice(DeviceInventoryGeneralDto device, CancellationToken ct);
        Task<DeviceInventoryGeneralDto> UpdateDevice(DeviceInventoryGeneralDto device, CancellationToken ct);
        Task<DeviceInventoryGeneralDto> ChangeDeviceState(int deviceId, bool state, CancellationToken ct);
        Task<DeviceInventoryGeneralDto?> GetDeviceById(int deviceId, CancellationToken ct);
        Task<IReadOnlyCollection<DeviceInventoryGeneralDto>> GetAllDevicesByMicrocontrollerId(int microCtrlId, int[]? devicesIds, CancellationToken ct);
    }
}
