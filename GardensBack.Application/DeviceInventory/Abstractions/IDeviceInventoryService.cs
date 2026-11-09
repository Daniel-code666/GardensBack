using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.DeviceInventory.Abstractions
{
    public interface IDeviceInventoryService
    {
        Task ProcessInventory(DeviceInventoryMessage inventory, CancellationToken ct);
        Task<DeviceInventoryGeneralDto> AddDevice(DeviceInventoryGeneralDto device, int requestedByUserId, CancellationToken ct);
        Task<DeviceInventoryGeneralDto> UpdateDevice(DeviceInventoryGeneralDto device, int requestedByUserId, CancellationToken ct);
        Task<bool> SetDeviceState(int[] deviceIds, bool state, int requestedByUserId, CancellationToken ct);
        Task<DeviceInventoryGeneralDto?> GetDeviceById(int deviceId, int requestedByUserId, CancellationToken ct);
        Task<DeviceInventoryGeneralDto?> GetDeviceByLocalId(GetDeviceByLocalIdDto request, int requestedByUserId, CancellationToken ct);
        Task<IReadOnlyCollection<DeviceInventoryGeneralDto>> GetAllDevicesByMicrocontrollerId(DeviceInventoryFilters request, int requestedByUserId, CancellationToken ct);
        Task ProcessTelemetry(string microcontrollerDeviceId,DeviceTelemetryMessage telemetry, CancellationToken ct);
        Task<IEnumerable<DeviceTelemetryDto>> GetTelemetryForDevices(GetTelemetryForDevicesDto devicesDto, int requestedByUserId, CancellationToken ct);
        Task<DeviceCommandExecutionResponseDto> CreateDeviceCommand(DeviceCommandRequestDto request, int requestedByUserId, bool waitForResponse,CancellationToken ct);
        Task<DeviceCommandResponseDto> GetDeviceCommandById(int commandId, int requestedByUserId, CancellationToken ct);
        Task<DeviceCommandResponseDto> GetDeviceCommandByRequest(string deviceId, string requestId, int requestedByUserId, CancellationToken ct);
        Task<DeviceCommandResponseDto> MarkDeviceCommandPublished(int commandId, CancellationToken ct);
        Task<DeviceCommandResponseDto> MarkDeviceCommandPublishFailed(int commandId, string errorCode, string errorMessage, CancellationToken ct);
        Task ProcessDeviceCommandResponse(string deviceId, DeviceActionResultMessage response, CancellationToken ct);
        Task<int> ExpireDeviceCommands(DateTime utcNow, CancellationToken ct);
    }
}
