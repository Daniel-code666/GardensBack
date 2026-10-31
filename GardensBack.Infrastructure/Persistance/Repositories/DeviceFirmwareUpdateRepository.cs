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

        public Task<DeviceFirmwareUpdate?> FindByRequest(string deviceId, string requestId, CancellationToken ct)
            => _db.FirmwareUpdates
                .Include(x => x.MicrocontrollerDevice)
                .Include(x => x.FirmwareRelease)
                .SingleOrDefaultAsync(x => x.RequestId == requestId && x.MicrocontrollerDevice.DeviceId == deviceId, ct);

        public Task<DeviceFirmwareUpdate?> FindAwaitingInventory(int deviceId, string firmwareVersion, CancellationToken ct)
            => _db.FirmwareUpdates
                .Include(x => x.FirmwareRelease)
                .SingleOrDefaultAsync(x =>
                    x.MicroCtlDeviceId == deviceId &&
                    x.Active &&
                    x.CompletedAt == null &&
                    x.Status != FirmwareUpdateStatus.Failed &&
                    x.FirmwareRelease.Version == firmwareVersion,
                    ct);

        public Task<int> CompleteConfirmedIfReady(int updateId, DateTime completedAt, CancellationToken ct)
            => _db.FirmwareUpdates
                .Where(x =>
                    x.Id == updateId &&
                    x.Active &&
                    x.CompletedAt == null &&
                    x.DeviceConfirmedAt != null &&
                    x.InventoryConfirmedAt != null &&
                    x.Status != FirmwareUpdateStatus.Failed)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, FirmwareUpdateStatus.Confirmed)
                    .SetProperty(x => x.CompletedAt, completedAt)
                    .SetProperty(x => x.ProgressPercentage, 100)
                    .SetProperty(x => x.FailureCode, (string?)null)
                    .SetProperty(x => x.FailureMessage, (string?)null)
                    .SetProperty(x => x.UpdateDate, completedAt), ct);

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
