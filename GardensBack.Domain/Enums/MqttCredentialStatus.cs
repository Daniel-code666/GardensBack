using System.Text.Json.Serialization;

namespace GardensBack.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MqttCredentialStatus : int
    {
        Pending, 
        Testing, 
        Active, 
        Revoked, 
        Failed
    }
}
