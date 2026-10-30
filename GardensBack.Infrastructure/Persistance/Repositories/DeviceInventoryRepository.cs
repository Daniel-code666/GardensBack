using GardensBack.Application.Abstractions;
using GardensBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GardensBack.Infrastructure.Persistance.Repositories
{
    public class DeviceInventoryRepository(ApplicationDbContext db) : IDeviceInventoryRepository
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<MicrocontrollerDevice?> FindByDeviceId(string deviceId, CancellationToken ct)
            => await _db.MicrocontrollerDevices.SingleOrDefaultAsync(x => x.DeviceId == deviceId, ct);

        public async Task<int> SaveChanges(CancellationToken ct)
            => await _db.SaveChangesAsync(ct);
    }
}
