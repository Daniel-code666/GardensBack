using GardensBack.Application.DeviceInventory.Dtos;

namespace GardensBack.Application.DeviceInventory.Abstractions
{
    public interface IDeviceActionPublisher
    {
        Task PublishCommand(string deviceId, DeviceActionMessage command, CancellationToken ct);
    }
}
