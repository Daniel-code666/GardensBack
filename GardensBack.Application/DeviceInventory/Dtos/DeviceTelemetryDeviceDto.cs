using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceTelemetryDeviceDto
    {
        public int Id { get; set; }
        public int MicrocontrollerDeviceId { get; set; }
        public int LocalDeviceId { get; set; }

        public required string Name { get; set; }
        public string? CustomName { get; set; }
        public string DisplayName => string.IsNullOrWhiteSpace(CustomName) ? Name : CustomName;

        public DeviceType DeviceType { get; set; }
        public bool Active { get; set; }
        public bool? Available { get; set; }

        public ConnectionType ConnectionType { get; set; }
        public int? DevicePin { get; set; }
        public int? I2cSdaPin { get; set; }
        public int? I2cSclPin { get; set; }
        public int? I2cAddress { get; set; }

        public MeasurementType? MeasurementType { get; set; }
        public MeasurementUnit? MeasurementUnit { get; set; }
    }
}
