using System.Text.Json.Serialization;

namespace GardensBack.Application.DeviceProvisioning.Enums
{
    /// <summary>
    /// Representa las decisiones que el firmware acepta en una respuesta de claim.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ClaimDecision
    {
        [JsonStringEnumMemberName("rejected")]
        Rejected,
        [JsonStringEnumMemberName("approved")]
        Approved,
        [JsonStringEnumMemberName("expired")]
        Expired
    }
}
