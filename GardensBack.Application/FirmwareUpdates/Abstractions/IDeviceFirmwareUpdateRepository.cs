using GardensBack.Domain.Entities;

namespace GardensBack.Application.FirmwareUpdates.Abstractions
{
    public interface IDeviceFirmwareUpdateRepository
    {
        Task<MicrocontrollerDevice?> FindDevice(string deviceId, CancellationToken ct);
        Task<DeviceFirmwareUpdate?> FindInProgress(int deviceId, CancellationToken ct);
        Task<DeviceFirmwareUpdate?> FindByRequest(string deviceId, string requestId, CancellationToken ct);
        Task<DeviceFirmwareUpdate?> FindAwaitingInventory(int deviceId, string firmwareVersion, CancellationToken ct);
        Task<int> CompleteConfirmedIfReady(int updateId, DateTime completedAt, CancellationToken ct);
        Task<uint> GetLastCommandVersion(int deviceId, CancellationToken ct);
        Task Add(DeviceFirmwareUpdate update, CancellationToken ct);
        Task<int> SaveChanges(CancellationToken ct);
    }
}
