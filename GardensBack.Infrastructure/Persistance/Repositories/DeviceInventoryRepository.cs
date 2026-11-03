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

        public async Task<Device> AddDevice(Device device, CancellationToken ct)
        {
            await _db.Devices.AddAsync(device, ct);
            return device;
        }

        public async Task<int> SaveChanges(CancellationToken ct)
            => await _db.SaveChangesAsync(ct);

        public async Task<IReadOnlyList<Device>> GetAllDevicesByMicrocontrollerId(int microCtrlId, int[]? devicesIds, CancellationToken ct)
            => await _db.Devices.Include(x => x.DeviceTelemetries)
                .Where(x => x.Active && x.MicroCtrlDeviceId == microCtrlId && (devicesIds == null || devicesIds.Contains(x.Id))).ToListAsync(ct);

        public async Task<Device?> GetDeviceById(int deviceId, CancellationToken ct)
            => await _db.Devices.Include(x => x.DeviceTelemetries).Include(x => x.MicroCtrlDevice)
                .SingleOrDefaultAsync(x => x.Id == deviceId && x.Active, ct);

        public async Task<MicrocontrollerDevice?> FindById(int microCtrlId, CancellationToken ct)
            => await _db.MicrocontrollerDevices.Include(x => x.Devices).SingleOrDefaultAsync(x => x.Id == microCtrlId, ct);
    }
}
