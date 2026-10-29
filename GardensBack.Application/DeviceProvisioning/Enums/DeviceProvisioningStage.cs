using System.Text.Json.Serialization;

namespace GardensBack.Application.DeviceProvisioning.Enums
{
    /// <summary>
    /// Identifica la etapa visible del proceso de configuración y enlace del ESP.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeviceProvisioningStage
    {
        [JsonStringEnumMemberName("identifying_device")]
        IdentifyingDevice,
        [JsonStringEnumMemberName("waiting_for_device")]
        WaitingForDevice,
        [JsonStringEnumMemberName("registering_device")]
        RegisteringDevice,
        [JsonStringEnumMemberName("waiting_for_claim")]
        WaitingForClaim,
        [JsonStringEnumMemberName("preparing_mqtt_credentials")]
        PreparingMqttCredentials,
        [JsonStringEnumMemberName("waiting_for_mqtt_confirmation")]
        WaitingForMqttConfirmation,
        [JsonStringEnumMemberName("completed")]
        Completed,
        [JsonStringEnumMemberName("failed")]
        Failed,
        [JsonStringEnumMemberName("expired")]
        Expired
    }
}
