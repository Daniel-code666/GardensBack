using GardensBack.Application.Common.Validation;
using GardensBack.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    /// <summary>
    /// Clase que representa los sensores/Actuadores conectados al microcontrolador
    /// </summary>
    public class DeviceInventoryItemMessage
    {
        [Range(1, int.MaxValue)]
        public int LocalDeviceId { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [EnumDataType(typeof(DeviceType))]
        public DeviceType DeviceType { get; set; }

        [Range(0, 48)]
        public int? Esp32Pin { get; set; }

        [EnumDataType(typeof(ConnectionType))]
        public ConnectionType ConnectionType { get; set; }

        [EnumDataType(typeof(MeasurementType))]
        public MeasurementType? MeasurementType { get; set; }

        [EnumDataType(typeof(MeasurementUnit))]
        public MeasurementUnit? MeasurementUnit { get; set; }

        public bool Available { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
