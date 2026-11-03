using GardensBack.Application.Common.Validation;
using GardensBack.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceInventoryGeneralDto
    {
        public int? Id { get; set; }
        public int LocalDeviceId { get; set; }
        [Required]
        public required string Name { get; set; }
        public int? DevicePin { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public MeasurementType? MeasurementType { get; set; }
        public MeasurementUnit? MeasurementUnit { get; set; }
        public DeviceType DeviceType { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
