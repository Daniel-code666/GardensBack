using GardensBack.Application.DeviceConnectivity.Abstractions;
using GardensBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GardensBack.Infrastructure.Persistance.Repositories
{
    public class DeviceConnectivityConfigurationRepository(ApplicationDbContext db) : IDeviceConnectivityConfigurationRepository
    {
        private readonly ApplicationDbContext _db = db;

        public Task<DeviceConnectivityConfiguration?> GetDefault(CancellationToken ct)
            => _db.DeviceConnectivityConfigurations.SingleOrDefaultAsync(x => x.ConfigurationKey == DeviceConnectivityConfiguration.DefaultConfigurationKey, ct);

        public Task Add(DeviceConnectivityConfiguration configuration, CancellationToken ct)
            => _db.DeviceConnectivityConfigurations.AddAsync(configuration, ct).AsTask();

        public Task<int> SaveChanges(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
