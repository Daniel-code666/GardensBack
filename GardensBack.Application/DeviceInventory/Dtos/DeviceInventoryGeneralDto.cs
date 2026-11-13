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
        [StringLength(100)]
        public string? CustomName { get; set; }
        public int? DevicePin { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public MeasurementType? MeasurementType { get; set; }
        public MeasurementUnit? MeasurementUnit { get; set; }
        public DeviceType DeviceType { get; set; }
        public required int MicrocontrollerDeviceId { get; set; }
        public int? I2cSdaPin { get; set; }
        public int? I2cSclPin { get; set; }
        public int? I2cAddress { get; set; }
        public bool Active { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public IReadOnlyCollection<DeviceTelemetryReadingDto> Telemetry { get; set; } = [];

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
