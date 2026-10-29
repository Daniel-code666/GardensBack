using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FirmwareReleaseStatus : int
    {
        Draft,
        Published,
        Revoked
    }
}
