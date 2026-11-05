using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceTelemetryReadingDto
    {
        public int Id { get; set; }
        public float Value { get; set; }
        public bool? State { get; set; }
        public ActuatorMode? ActuatorMode { get; set; }
        public TelemetrySource Source { get; set; }
        public string? RequestId { get; set; }
        public DateTime RecordedAtUtc { get; set; }
    }
}
