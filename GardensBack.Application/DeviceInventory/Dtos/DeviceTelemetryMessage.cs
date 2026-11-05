using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceTelemetryMessage
    {
        public List<SensorReadingMessage> SensorReadings { get; set; } = [];

        public List<ActuatorReadingMessage> ActuatorReadings { get; set; } = [];
    }

    public class SensorReadingMessage
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required MeasurementType MeasurementType { get; set; }

        public required MeasurementUnit MeasurementUnit { get; set; }

        public int Esp32Pin { get; set; }

        public required ConnectionType ConnType { get; set; }

        public double Value { get; set; }
    }

    public class ActuatorReadingMessage
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public int Esp32Pin { get; set; }

        public required ConnectionType ConnType { get; set; }

        public required ActuatorMode Mode { get; set; }

        public int State { get; set; }
    }
}
