using GardensBack.Application.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;
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
            => await _db.Devices.AsNoTracking().Include(x => x.DeviceTelemetries.OrderByDescending(x => x.CreationDate).Take(request.TelemtryPerRequest))
                .Where(x => x.Active == request.Active && x.Available == request.Avaliable
                && x.MicroCtrlDeviceId == request.SelectedMicrocontrollerId && (request.SelectedDeviceIds == null || request.SelectedDeviceIds.Contains(x.Id))).ToListAsync(ct);

        public async Task<IReadOnlyList<MicrocontrollerDevice>> GetMicroControllerDeviceByIds(int[] ids, CancellationToken ct)
            => await _db.MicrocontrollerDevices.AsNoTracking().OrderBy(x => x.Id).Where(x => ids.Contains(x.Id)).ToListAsync(ct);

        public async Task<MicrocontrollerDevice?> GetMicroControllerDeviceById(int id, CancellationToken ct)
            => await _db.MicrocontrollerDevices.AsNoTracking().Include(x => x.User).Include(x => x.Devices).Where(x => x.Id == id).SingleOrDefaultAsync(ct);

        public async Task<IReadOnlyList<DeviceTelemetry>> GetTelemetryByDeviceId(int id, int itemPerRequest, CancellationToken ct)
            => await _db.DeviceTelemetries.AsNoTracking().OrderByDescending(t => t.CreationDate).Take(itemPerRequest).Where(x => x.DeviceId == id).ToListAsync(ct);

        public async Task<Device?> GetDeviceById(int deviceId, CancellationToken ct)
            => await _db.Devices.Include(x => x.MicroCtrlDevice).SingleOrDefaultAsync(x => x.Id == deviceId, ct);

        public async Task<IEnumerable<Device>> GetDevicesByIds(int[] deviceIds, CancellationToken ct)
            => await _db.Devices
                .Include(x => x.MicroCtrlDevice)
                .ThenInclude(x => x.MqttCredential)
                .Where(x => deviceIds.Contains(x.Id))
                .ToListAsync(ct);

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
            => await _db.Devices.AsNoTracking()
                .Include(x => x.MicroCtrlDevice)
                .Include(x => x.DeviceTelemetries.OrderByDescending(t => t.CreationDate).Take(itemPerRequest))
                .Where(x => devicesId.Contains(x.Id)).ToListAsync(ct);

        public async Task<bool> CheckMicroCtrlDeviceUserOwnership(int deviceId, int microControllerId, int userId, CancellationToken ct)
            => await _db.MicrocontrollerDevices.AnyAsync(x => x.Id == microControllerId && x.UserId == userId && x.Devices.Any(d => d.Id == deviceId), ct);

        public async Task<IEnumerable<DeviceTelemetry>> GetTelemetryForDevices(int[] devicesIds, int itemPerRequest, CancellationToken ct)
            => await _db.DeviceTelemetries.AsNoTracking().Where(x => devicesIds.Contains(x.DeviceId)).OrderByDescending(x => x.CreationDate).Take(itemPerRequest).ToListAsync(ct);

        public Task AddCommand(DeviceCommand command, CancellationToken ct)
            => _db.DeviceCommands.AddAsync(command, ct).AsTask();

        public Task<DeviceCommand?> GetCommandById(int commandId, CancellationToken ct)
            => _db.DeviceCommands
                .Include(x => x.MicroCtrlDevice)
                .SingleOrDefaultAsync(x => x.Id == commandId, ct);

        public Task<DeviceCommand?> GetCommandByRequest(string deviceId, string requestId, CancellationToken ct)
            => _db.DeviceCommands
                .Include(x => x.MicroCtrlDevice)
                .SingleOrDefaultAsync(x =>
                    x.RequestId == requestId &&
                    x.MicroCtrlDevice.DeviceId == deviceId,
                    ct);

        public async Task<uint> GetLastCommandVersion(int microcontrollerId, CancellationToken ct)
            => await _db.DeviceCommands
                .Where(x => x.MicroCtrlDeviceId == microcontrollerId)
                .MaxAsync(x => (uint?)x.CommandVersion, ct) ?? 0;

        public Task<int> MarkCommandPublished(int commandId, DateTime publishedAt, CancellationToken ct)
            => _db.DeviceCommands
                .Where(x => x.Id == commandId && x.Status == CommandStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, CommandStatus.Published)
                    .SetProperty(x => x.PublishedAt, publishedAt)
                    .SetProperty(x => x.ErrorCode, (string?)null)
                    .SetProperty(x => x.ErrorMessage, (string?)null)
                    .SetProperty(x => x.UpdateDate, publishedAt), ct);

        public Task ReloadCommand(DeviceCommand command, CancellationToken ct)
            => _db.Entry(command).ReloadAsync(ct);

        public Task<int> ExpireCommands(DateTime utcNow, CancellationToken ct)
            => _db.DeviceCommands
                .Where(x =>
                    x.Active &&
                    x.ExpiresAt <= utcNow &&
                    (x.Status == CommandStatus.Pending || x.Status == CommandStatus.Published))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, CommandStatus.Expired)
                    .SetProperty(x => x.Success, (bool?)null)
                    .SetProperty(x => x.ErrorCode, "command_response_timeout")
                    .SetProperty(x => x.ErrorMessage, "No se recibió confirmación del dispositivo antes de la fecha de expiración.")
                    .SetProperty(x => x.CompletedAt, utcNow)
                    .SetProperty(x => x.UpdateDate, utcNow), ct);

        public async Task<int> SaveChanges(CancellationToken ct)
            => await _db.SaveChangesAsync(ct);

    }
}
