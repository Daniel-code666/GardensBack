using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class TelemetryRealtimeDto
    {
        public required string MicrocontrollerDeviceId { get; set; }

        public IReadOnlyCollection<TelemetryRealtimeReadingDto> Readings { get; set; } = [];
    }

    public class TelemetryRealtimeReadingDto
    {
        public int DeviceId { get; set; }

        public float Value { get; set; }

        public bool? State { get; set; }

        public ActuatorMode? ActuatorMode { get; set; }

        public TelemetrySource Source { get; set; }

        public string? RequestId { get; set; }

        public DateTime RecordedAtUtc { get; set; }
    }
}
