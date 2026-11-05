using GardensBack.Application.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GardensBack.Infrastructure.Persistance.Repositories
{
    public class DeviceInventoryRepository(ApplicationDbContext db) : IDeviceInventoryRepository
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<MicrocontrollerDevice?> FindByDeviceId(string deviceId, bool includeDevices, CancellationToken ct)
        {
            var query = _db.MicrocontrollerDevices.AsQueryable();
            if (includeDevices)
                query = query.Include(x => x.Devices);

            return await query.SingleOrDefaultAsync(x => x.DeviceId == deviceId, ct);
        }

        public async Task<Device> AddDevice(Device device, CancellationToken ct)
        {
            await _db.Devices.AddAsync(device, ct);
            return device;
        }

        public async Task<IReadOnlyList<Device>> GetAllDevicesByMicrocontrollerId(DeviceInventoryFilters request, CancellationToken ct)
            => await _db.Devices.Include(x => x.DeviceTelemetries)
                .Where(x => x.Active == request.Active && x.Available == request.Avaliable
                && x.MicroCtrlDeviceId == request.SelectedMicrocontrollerId && (request.SelectedDeviceIds == null || request.SelectedDeviceIds.Contains(x.Id))).ToListAsync(ct);

        public async Task<Device?> GetDeviceById(int deviceId, CancellationToken ct)
            => await _db.Devices.Include(x => x.MicroCtrlDevice).SingleOrDefaultAsync(x => x.Id == deviceId, ct);

        public async Task<IEnumerable<Device>> GetDevicesByIds(int[] deviceIds, CancellationToken ct)
            =>  await _db.Devices.Where(x => deviceIds.Contains(x.Id)).ToListAsync(ct);

        public async Task<MicrocontrollerDevice?> FindById(int microCtrlId, bool includeDevices, CancellationToken ct)
        {
            var query = _db.MicrocontrollerDevices.AsQueryable();
            if (includeDevices)
                query = query.Include(x => x.Devices);

            return await query.SingleOrDefaultAsync(x => x.Id == microCtrlId, ct);
        }

        public async Task<Device?> GetDeviceByLocalId(GetDeviceByLocalIdDto request, CancellationToken ct)
            => await _db.Devices.Include(x => x.MicroCtrlDevice)
                .SingleOrDefaultAsync(x => x.LocalDeviceId == request.LocalDeviceId && x.MicroCtrlDeviceId == request.MicrocontrollerId
                    && x.DeviceType == request.DeviceType && (request.MeasurementType == null || x.MeasurementType == request.MeasurementType), ct);

        public async Task AddTelemetry(DeviceTelemetry telemetry, CancellationToken ct)
            => await _db.DeviceTelemetries.AddAsync(telemetry, ct);

        public async Task<IEnumerable<Device>> GetDevicesWithTelemetry(int[] devicesId, int itemPerRequest, CancellationToken ct)
            => await _db.Devices.AsNoTracking().Include(x => x.DeviceTelemetries.OrderByDescending(t => t.CreationDate).Take(itemPerRequest))
                .Where(x => devicesId.Contains(x.Id)).ToListAsync(ct);

        public async Task<IEnumerable<DeviceTelemetry>> GetTelemetryForDevices(int[] devicesIds, int itemPerRequest, CancellationToken ct)
            => await _db.DeviceTelemetries.AsNoTracking().Where(x => devicesIds.Contains(x.DeviceId)).OrderByDescending(x => x.CreationDate).Take(itemPerRequest).ToListAsync(ct);

        public async Task<int> SaveChanges(CancellationToken ct)
            => await _db.SaveChangesAsync(ct);

    }
}
