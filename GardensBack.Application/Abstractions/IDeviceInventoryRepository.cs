using GardensBack.Domain.Entities;

namespace GardensBack.Application.Abstractions
{
    public interface IDeviceInventoryRepository
    {
        Task<MicrocontrollerDevice?> FindByDeviceId(string deviceId, CancellationToken ct);
        Task<int> SaveChanges(CancellationToken ct);
    }
}
