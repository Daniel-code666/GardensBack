using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GardensBack.Infrastructure.Persistance.Repositories
{
    public class DeviceFirmwareUpdateRepository(ApplicationDbContext db) : IDeviceFirmwareUpdateRepository
    {
        private readonly ApplicationDbContext _db = db;

        public Task<MicrocontrollerDevice?> FindDevice(string deviceId, CancellationToken ct)
            => _db.MicrocontrollerDevices
                .Include(x => x.User)
                .Include(x => x.MqttCredential)
                .SingleOrDefaultAsync(x => x.DeviceId == deviceId, ct);

        public Task<DeviceFirmwareUpdate?> FindInProgress(int deviceId, CancellationToken ct)
            => _db.FirmwareUpdates.SingleOrDefaultAsync(x =>
                x.MicroCtlDeviceId == deviceId &&
                x.Active &&
                (x.Status == FirmwareUpdateStatus.Pending ||
                 x.Status == FirmwareUpdateStatus.Downloading ||
                 x.Status == FirmwareUpdateStatus.PendingValidation), ct);

        public async Task<uint> GetLastCommandVersion(int deviceId, CancellationToken ct)
            => await _db.FirmwareUpdates
                .Where(x => x.MicroCtlDeviceId == deviceId)
                .MaxAsync(x => (uint?)x.CommandVersion, ct) ?? 0;

        public Task Add(DeviceFirmwareUpdate update, CancellationToken ct)
            => _db.FirmwareUpdates.AddAsync(update, ct).AsTask();

        public Task<int> SaveChanges(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
