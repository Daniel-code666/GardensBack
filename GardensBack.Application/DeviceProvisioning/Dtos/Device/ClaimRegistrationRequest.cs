namespace GardensBack.Application.DeviceProvisioning.Dtos.Device
{
    using GardensBack.Application.Common.Validation;
    using GardensBack.Application.DeviceProvisioning.Constants;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Representa la identidad y la solicitud de claim que el ESP registra directamente en el backend.
    /// </summary>
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class ClaimRegistrationRequest
    {
        [Required]
        [RegularExpression("^" + DeviceProvisioningContract.ClaimRegistrationType + "$")]
        public required string Type { get; set; }

        [Range(DeviceProvisioningContract.Version, DeviceProvisioningContract.Version)]
        public int ContractVersion { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string DeviceId { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.HardwareIdPattern)]
        public required string HardwareId { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string MqttClientId { get; set; }

        [Required, StringLength(64, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string Board { get; set; }

        [Required, StringLength(32, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.FirmwareVersionPattern)]
        public required string FirmwareVersion { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.RequestIdPattern)]
        public required string RequestId { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.ClaimCodePattern)]
        public required string ClaimCode { get; set; }

        [Range(1, DeviceProvisioningContract.MaximumClaimLifetimeSeconds)]
        public int ExpiresInSeconds { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
