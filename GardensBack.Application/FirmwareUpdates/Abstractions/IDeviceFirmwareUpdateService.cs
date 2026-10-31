using GardensBack.Application.FirmwareUpdates.Dtos;

namespace GardensBack.Application.FirmwareUpdates.Abstractions
{
    public interface IDeviceFirmwareUpdateService
    {
        Task<DeviceFirmwareUpdateResponse> RequestUpdate(RequestDeviceFirmwareUpdateRequest request, int requestedByUserId, CancellationToken ct);
    }
}
