using System.Text.Json.Serialization;

namespace GardensBack.Application.DeviceProvisioning.Enums
{
    /// <summary>
    /// Representa el estado global del flujo administrado, no el estado persistido por el ESP.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProvisioningWorkflowStatus
    {
        [JsonStringEnumMemberName("unprovisioned")]
        Unprovisioned,
        [JsonStringEnumMemberName("provisioning")]
        Provisioning,
        [JsonStringEnumMemberName("provisioned")]
        Provisioned,
        [JsonStringEnumMemberName("failed")]
        Failed
    }
}
