namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class GetTelemetryForDevicesDto
    {
        public int[] DevicesIds { get; set; } = [];
        [System.ComponentModel.DataAnnotations.Range(1, 100)]
        public int ItemPerRequest { get; set; } = 20;
    }
}
