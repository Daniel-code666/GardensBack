using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MeasurementType : int
    {
        SoilTemperature,
        AirTemperature,
        AirHumidity,
        Illuminance,
        SoilMoisture
    }
}
