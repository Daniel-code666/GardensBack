using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;

namespace GardensBack.Application.Abstractions
{
    public interface IDeviceInventoryRepository
    {
        Task<MicrocontrollerDevice?> FindByDeviceId(string deviceId, bool includeDevices, CancellationToken ct);
        Task<MicrocontrollerDevice?> FindById(int microCtrlId, bool includeDevices, CancellationToken ct);
        Task<Device> AddDevice(Device device, CancellationToken ct);
        Task<IReadOnlyList<Device>> GetAllDevicesByMicrocontrollerId(DeviceInventoryFilters request, CancellationToken ct);
        Task<Device?> GetDeviceById(int deviceId, CancellationToken ct);
        Task<IEnumerable<Device>> GetDevicesByIds(int[] deviceIds, CancellationToken ct);
        Task<Device?> GetDeviceByLocalId(GetDeviceByLocalIdDto request, CancellationToken ct);
        Task AddTelemetry(DeviceTelemetry telemetry, CancellationToken ct);
        Task<IEnumerable<Device>> GetDevicesWithTelemetry(int[] devicesIds, int itemPerRequest, CancellationToken ct);
        Task<IEnumerable<DeviceTelemetry>> GetTelemetryForDevices(int[] devicesIds, int itemPerRequest, CancellationToken ct);
        Task AddCommand(DeviceCommand command, CancellationToken ct);
        Task<DeviceCommand?> GetCommandById(int commandId, CancellationToken ct);
        Task<DeviceCommand?> GetCommandByRequest(string deviceId, string requestId, CancellationToken ct);
        Task<uint> GetLastCommandVersion(int microcontrollerId, CancellationToken ct);
        Task<int> MarkCommandPublished(int commandId, DateTime publishedAt, CancellationToken ct);
        Task ReloadCommand(DeviceCommand command, CancellationToken ct);
        Task<int> ExpireCommands(DateTime utcNow, CancellationToken ct);
        Task<int> SaveChanges(CancellationToken ct);
    }
}
