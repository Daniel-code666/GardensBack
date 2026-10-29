using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TelemetrySource : int
    {
        PeriodicTelemetry,
        CommandResponse
    }
}
