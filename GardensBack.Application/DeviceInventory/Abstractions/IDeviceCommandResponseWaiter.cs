using GardensBack.Application.DeviceInventory.Dtos;

namespace GardensBack.Application.DeviceInventory.Abstractions
{
    public interface IDeviceCommandResponseWaiter
    {
        Task<DeviceActionResultMessage?> WaitAsync(string requestId, TimeSpan timeout, CancellationToken ct);

        bool Complete(string requestId, DeviceActionResultMessage response);

        void Cancel(string requestId);
    }
}
