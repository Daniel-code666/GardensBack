using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.DeviceInventory.Abstractions
{
    public interface IDeviceInventoryService
    {
        Task ProcessInventory(DeviceInventoryMessage inventory, CancellationToken ct);
        Task<DeviceInventoryGeneralDto> AddDevice(DeviceInventoryGeneralDto device, CancellationToken ct);
        Task<DeviceInventoryGeneralDto> UpdateDevice(DeviceInventoryGeneralDto device, CancellationToken ct);
        Task<bool> SetDeviceState(int[] deviceIds, bool state, CancellationToken ct);
        Task<DeviceInventoryGeneralDto?> GetDeviceById(int deviceId, CancellationToken ct);
        Task<DeviceInventoryGeneralDto?> GetDeviceByLocalId(GetDeviceByLocalIdDto request, CancellationToken ct);
        Task<IReadOnlyCollection<DeviceInventoryGeneralDto>> GetAllDevicesByMicrocontrollerId(DeviceInventoryFilters request, CancellationToken ct);
        Task ProcessTelemetry(string microcontrollerDeviceId,DeviceTelemetryMessage telemetry, CancellationToken ct);
        Task<IEnumerable<DeviceTelemetryDto>> GetTelemetryForDevices(GetTelemetryForDevicesDto devicesDto, CancellationToken ct);
        Task<DeviceCommandResponseDto> CreateDeviceCommand(DeviceCommandRequestDto request, CancellationToken ct);
        Task<DeviceCommandResponseDto> GetDeviceCommandById(int commandId, CancellationToken ct);
        Task<DeviceCommandResponseDto> GetDeviceCommandByRequest(string deviceId, string requestId, CancellationToken ct);
        Task<DeviceCommandResponseDto> MarkDeviceCommandPublished(int commandId, CancellationToken ct);
        Task<DeviceCommandResponseDto> MarkDeviceCommandPublishFailed(int commandId, string errorCode, string errorMessage, CancellationToken ct);
        Task ProcessDeviceCommandResponse(string deviceId, DeviceActionResultMessage response, CancellationToken ct);
        Task<int> ExpireDeviceCommands(DateTime utcNow, CancellationToken ct);
    }
}
