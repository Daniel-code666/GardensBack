using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CommandStatus : int
    {
        Pending,
        Published,
        Completed,
        Failed,
        Expired
    }
}
