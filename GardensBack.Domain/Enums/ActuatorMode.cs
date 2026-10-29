using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActuatorMode : int
    {
        Automatic,
        Manual
    }
}
