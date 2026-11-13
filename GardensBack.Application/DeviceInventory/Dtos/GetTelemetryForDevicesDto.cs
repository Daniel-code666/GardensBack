namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class GetTelemetryForDevicesDto
    {
        public int[] DevicesIds { get; set; } = [];
        public int ItemPerRequest { get; set; } = 20;
        public DateTimeOffset? StartDateUtc { get; set; }
        public DateTimeOffset? EndDateUtc { get; set; }
    }
}
