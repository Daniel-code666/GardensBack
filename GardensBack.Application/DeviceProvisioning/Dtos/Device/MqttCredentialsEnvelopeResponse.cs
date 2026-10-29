namespace GardensBack.Application.DeviceProvisioning.Dtos.Device
{
    using GardensBack.Application.Common.Validation;
    using GardensBack.Application.DeviceProvisioning.Constants;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Contiene el sobre AES-256-GCM que permite entregar las credenciales MQTT sin exponerlas en texto plano.
    /// </summary>
    public class MqttCredentialsEnvelopeResponse
    {
        [Required]
        [RegularExpression("^" + DeviceProvisioningContract.MqttCredentialsEnvelopeType + "$")]
        public string Type { get; set; } = DeviceProvisioningContract.MqttCredentialsEnvelopeType;

        [Range(DeviceProvisioningContract.Version, DeviceProvisioningContract.Version)]
        public int ContractVersion { get; set; } = DeviceProvisioningContract.Version;

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string DeviceId { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.RequestIdPattern)]
        public required string RequestId { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.NoncePattern)]
        public required string Nonce { get; set; }

        [Required, StringLength(4096, MinimumLength = 2)]
        [RegularExpression(DeviceProvisioningContract.CiphertextPattern)]
        public required string Ciphertext { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.AuthenticationTagPattern)]
        public required string AuthenticationTag { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
