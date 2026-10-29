using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MeasurementUnit : int
    {
        Celsius,
        Lux,
        Percentage
    }
}
