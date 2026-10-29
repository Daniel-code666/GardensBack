using GardensBack.Domain.Entities;

namespace GardensBack.Application.DeviceConnectivity.Abstractions
{
    public interface IDeviceConnectivityConfigurationRepository
    {
        Task<DeviceConnectivityConfiguration?> GetDefault(CancellationToken ct);
        Task Add(DeviceConnectivityConfiguration configuration, CancellationToken ct);
        Task<int> SaveChanges(CancellationToken ct);
    }
}
