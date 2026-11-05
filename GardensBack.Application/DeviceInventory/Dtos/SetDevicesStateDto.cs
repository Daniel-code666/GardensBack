namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class SetDevicesStateDto
    {
        public int[] DevicesIds { get; set; } = [];
        public bool State { get; set; }
    }
}
