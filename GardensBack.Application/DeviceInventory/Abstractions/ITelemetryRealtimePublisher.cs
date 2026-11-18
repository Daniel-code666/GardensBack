using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.DeviceInventory.Abstractions
{
    public interface ITelemetryRealtimePublisher
    {
        Task PublishAsync(TelemetryRealtimeDto telemetry, CancellationToken ct);
    }
}
