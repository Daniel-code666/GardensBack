using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Constants
{
    public class Constants
    {
        public static readonly HashSet<(MeasurementType Type, MeasurementUnit Unit)> ValidMeasurements =
        [
            (MeasurementType.SoilTemperature, MeasurementUnit.Celsius),
            (MeasurementType.AirTemperature, MeasurementUnit.Celsius),
            (MeasurementType.AirHumidity, MeasurementUnit.Percentage),
            (MeasurementType.Illuminance, MeasurementUnit.Lux),
            (MeasurementType.SoilMoisture, MeasurementUnit.Percentage)
        ];
    }
}
