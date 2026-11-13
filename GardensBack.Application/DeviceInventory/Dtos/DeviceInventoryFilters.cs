using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceInventoryFilters
    {
        public int[]? SelectedDeviceIds { get; set; }
        public int SelectedMicrocontrollerId { get; set; }
        public bool Active { get; set; }
        public bool Avaliable { get; set; }
        public DeviceType DeviceType { get; set; }
        public MeasurementType MeasurementType { get; set; }
        public MeasurementUnit MeasurementUnit { get; set; }
        public int TelemtryPerRequest { get; set; } = 20;
        public DateTimeOffset? StartDateUtc { get; set; }
        public DateTimeOffset? EndDateUtc { get; set; }
    }
}
