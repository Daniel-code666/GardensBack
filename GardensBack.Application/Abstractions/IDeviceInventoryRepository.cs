using GardensBack.Domain.Entities;

namespace GardensBack.Application.Abstractions
{
    public interface IDeviceInventoryRepository
    {
        Task<MicrocontrollerDevice?> FindByDeviceId(string deviceId, bool includeDevices, CancellationToken ct);
        Task<MicrocontrollerDevice?> FindById(int microCtrlId, CancellationToken ct);
        Task<Device> AddDevice(Device device, CancellationToken ct);
        Task<IReadOnlyList<Device>> GetAllDevicesByMicrocontrollerId(int microCtrlId, int[]? devicesIds, CancellationToken ct);
        Task<Device?> GetDeviceById(int deviceId, CancellationToken ct);
        Task<int> SaveChanges(CancellationToken ct);
    }
}
