using GardensBack.Application.FirmwareUpdates.Dtos;

namespace GardensBack.Application.FirmwareUpdates.Abstractions
{
    public interface IDeviceControlPublisher
    {
        Task Publish(string deviceId, OtaUpdateCommand command, CancellationToken ct);
    }
}
