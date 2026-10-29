using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ConnectionType : int
    {
        OneWire,
        I2C,
        Analog,
        Digital
    }
}
