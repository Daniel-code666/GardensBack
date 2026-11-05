using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class GetDeviceByLocalIdDto
    {
        public int LocalDeviceId { get; set; }
        public int MicrocontrollerId { get; set; }
        public DeviceType DeviceType { get; set; }
        public MeasurementType? MeasurementType { get; set; }
    }
}
