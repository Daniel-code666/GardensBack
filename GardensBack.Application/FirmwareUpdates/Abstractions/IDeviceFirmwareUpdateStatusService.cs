using GardensBack.Application.FirmwareUpdates.Dtos;

namespace GardensBack.Application.FirmwareUpdates.Abstractions
{
    public interface IDeviceFirmwareUpdateStatusService
    {
        Task ProcessStatus(string topicDeviceId, OtaUpdateStatusMessage message, CancellationToken ct);
    }
}
