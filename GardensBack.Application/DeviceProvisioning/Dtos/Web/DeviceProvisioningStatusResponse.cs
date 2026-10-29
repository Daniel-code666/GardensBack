namespace GardensBack.Application.DeviceProvisioning.Dtos.Web
{
    using GardensBack.Application.Common.Validation;
    using GardensBack.Application.DeviceProvisioning.Constants;
    using GardensBack.Application.DeviceProvisioning.Enums;
    using GardensBack.Domain.Enums;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Es un DTO de salida para que la web consulte el progreso.
    /// </summary>
    public class DeviceProvisioningStatusResponse
    {
        public Guid DevicePublicId { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string DeviceId { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumBootstrapRequestIdLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string BootstrapRequestId { get; set; }

        [EnumDataType(typeof(DeviceProvisioningStage))]
        public DeviceProvisioningStage Stage { get; set; }

        [EnumDataType(typeof(ProvisioningWorkflowStatus))]
        public ProvisioningWorkflowStatus Status { get; set; }

        [EnumDataType(typeof(ProvisioningState))]
        public ProvisioningState ProvisioningState { get; set; }

        [StringLength(32)]
        [RegularExpression(DeviceProvisioningContract.FirmwareVersionPattern)]
        public string? FirmwareVersion { get; set; }

        public bool? WifiConnected { get; set; }

        public bool? MqttConnected { get; set; }

        public uint? MqttCredentialVersion { get; set; }

        [StringLength(DeviceProvisioningContract.MaximumErrorCodeLength)]
        [RegularExpression(DeviceProvisioningContract.ErrorCodePattern)]
        public string? FailureCode { get; set; }

        public DateTime StartedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? CompletedAtUtc { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
