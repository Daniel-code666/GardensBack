using GardensBack.Application.DeviceInventory.Dtos;

namespace GardensBack.Application.DeviceInventory.Abstractions
{
    public interface IDeviceInventoryService
    {
        Task ProcessInventory(DeviceInventoryMessage inventory, CancellationToken ct);
        Task<DeviceInventoryGeneralDto> AddDevice(DeviceInventoryGeneralDto device, CancellationToken ct);
        Task<DeviceInventoryGeneralDto> UpdateDevice(DeviceInventoryGeneralDto device, CancellationToken ct);
        Task<bool> SetDeviceState(int[] deviceIds, bool state, CancellationToken ct);
        Task<DeviceInventoryGeneralDto?> GetDeviceById(int deviceId, CancellationToken ct);
        Task<DeviceInventoryGeneralDto?> GetDeviceByLocalId(GetDeviceByLocalIdDto request, CancellationToken ct);
        Task<IReadOnlyCollection<DeviceInventoryGeneralDto>> GetAllDevicesByMicrocontrollerId(DeviceInventoryFilters request, CancellationToken ct);
    }
}
