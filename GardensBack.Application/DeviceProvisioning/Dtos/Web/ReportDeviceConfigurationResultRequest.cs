using GardensBack.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.DeviceProvisioning.Dtos.Web
{
    using GardensBack.Application.Common.Validation;
    using GardensBack.Application.DeviceProvisioning.Constants;
    using System.Text.Json.Serialization;

    /// <summary>
    /// La aplicación reenvía al backend el resultado final recibido por Web Serial.
    /// </summary>
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class ReportDeviceConfigurationResultRequest : IValidatableObject
    {
        [Required]
        [RegularExpression("^" + DeviceProvisioningContract.DeviceConfigurationResultType + "$")]
        public required string Type { get; set; }

        [Range(DeviceProvisioningContract.Version, DeviceProvisioningContract.Version)]
        public int ContractVersion { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumBootstrapRequestIdLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string RequestId { get; set; }

        [JsonRequired]
        public bool Success { get; set; }

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string DeviceId { get; set; }

        [Required, StringLength(32, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.FirmwareVersionPattern)]
        public required string FirmwareVersion { get; set; }

        [JsonRequired, EnumDataType(typeof(ProvisioningState))]
        public ProvisioningState ProvisioningState { get; set; }

        [JsonRequired]
        public bool WifiConnected { get; set; }

        [JsonRequired]
        public bool MqttConnected { get; set; }

        [JsonRequired]
        public uint MqttCredentialVersion { get; set; }

        [StringLength(DeviceProvisioningContract.MaximumErrorCodeLength)]
        [RegularExpression(DeviceProvisioningContract.ErrorCodePattern)]
        public string? FailureCode { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Success)
            {
                if (!WifiConnected || !MqttConnected)
                    yield return new ValidationResult("Una configuración exitosa debe confirmar Wi-Fi y MQTT.", [nameof(WifiConnected), nameof(MqttConnected)]);
                if (MqttCredentialVersion == 0)
                    yield return new ValidationResult("Una configuración exitosa debe informar una versión MQTT mayor que cero.", [nameof(MqttCredentialVersion)]);
                if (!string.IsNullOrEmpty(FailureCode))
                    yield return new ValidationResult("FailureCode debe estar vacío cuando la configuración es exitosa.", [nameof(FailureCode)]);
            }
            else if (string.IsNullOrWhiteSpace(FailureCode))
            {
                yield return new ValidationResult("FailureCode es obligatorio cuando la configuración falla.", [nameof(FailureCode)]);
            }
        }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
