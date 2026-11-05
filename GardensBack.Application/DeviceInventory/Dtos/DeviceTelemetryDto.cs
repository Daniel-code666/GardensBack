namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceTelemetryDto
    {
        public required DeviceTelemetryDeviceDto Device { get; set; }
        public IReadOnlyCollection<DeviceTelemetryReadingDto> Telemetry { get; set; } = [];
    }
}
