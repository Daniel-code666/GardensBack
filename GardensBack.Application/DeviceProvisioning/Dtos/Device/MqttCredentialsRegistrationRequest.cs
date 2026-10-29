namespace GardensBack.Application.DeviceProvisioning.Dtos.Device
{
    using GardensBack.Application.Common.Validation;
    using GardensBack.Application.DeviceProvisioning.Constants;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Representa la solicitud con la que un ESP reclamado registra su material temporal para recibir credenciales MQTT.
    /// </summary>
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class MqttCredentialsRegistrationRequest
    {
        [Required]
        [RegularExpression("^" + DeviceProvisioningContract.MqttCredentialsRegistrationType + "$")]
        public required string Type { get; set; }

        [Range(DeviceProvisioningContract.Version, DeviceProvisioningContract.Version)]
        public int ContractVersion { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string DeviceId { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.RequestIdPattern)]
        public required string RequestId { get; set; }

        [JsonRequired]
        public uint CurrentCredentialVersion { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.HexadecimalKeyPattern)]
        public required string BootstrapKey { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
