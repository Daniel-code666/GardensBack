using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;

namespace GardensBack.Application.Abstractions
{
    public interface IDeviceInventoryRepository
    {
        Task<MicrocontrollerDevice?> FindByDeviceId(string deviceId, bool includeDevices, CancellationToken ct);
        Task<MicrocontrollerDevice?> FindById(int microCtrlId, bool includeDevices, CancellationToken ct);
        Task<Device> AddDevice(Device device, CancellationToken ct);
        Task<IReadOnlyList<Device>> GetAllDevicesByMicrocontrollerId(DeviceInventoryFilters request, CancellationToken ct);
        Task<Device?> GetDeviceById(int deviceId, CancellationToken ct);
        Task<IEnumerable<Device>> GetDevicesByIds(int[] deviceIds, CancellationToken ct);
        Task<Device?> GetDeviceByLocalId(GetDeviceByLocalIdDto request, CancellationToken ct);
        Task<int> SaveChanges(CancellationToken ct);
    }
}
