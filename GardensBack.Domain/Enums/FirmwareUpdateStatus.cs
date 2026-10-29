using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FirmwareUpdateStatus : int
    {
        Pending,
        Downloading,
        PendingValidation,
        Confirmed,
        Failed
    }
}
