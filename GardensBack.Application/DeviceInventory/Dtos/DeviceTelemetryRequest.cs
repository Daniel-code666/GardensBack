namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceTelemetryRequest
    {
        public int DeviceId { get; set; }
        public int MicrocontrollerId { get; set; }
        public int ItemPerRequest { get; set; } = 20;
        public DateTimeOffset? StartDateUtc { get; set; }
        public DateTimeOffset? EndDateUtc { get; set; }
    }
}
