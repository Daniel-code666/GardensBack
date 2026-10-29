namespace GardensBack.Application.DeviceProvisioning.Dtos.Device
{
    using GardensBack.Application.Common.Validation;
    using GardensBack.Application.DeviceProvisioning.Constants;
    using GardensBack.Application.DeviceProvisioning.Enums;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Contiene la decisión autenticada que el ESP necesita para completar o cancelar su claim.
    /// </summary>
    public class ClaimResponse : IValidatableObject
    {
        [Required]
        [RegularExpression("^" + DeviceProvisioningContract.ClaimResponseType + "$")]
        public string Type { get; set; } = DeviceProvisioningContract.ClaimResponseType;

        [Range(DeviceProvisioningContract.Version, DeviceProvisioningContract.Version)]
        public int ContractVersion { get; set; } = DeviceProvisioningContract.Version;

        [Required, StringLength(DeviceProvisioningContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public required string DeviceId { get; set; }

        [Required, RegularExpression(DeviceProvisioningContract.RequestIdPattern)]
        public required string RequestId { get; set; }

        [EnumDataType(typeof(ClaimDecision))]
        public ClaimDecision Decision { get; set; }

        [StringLength(DeviceProvisioningContract.MaximumIdentifierLength)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(DeviceProvisioningContract.MaximumIdentifierLength)]
        [RegularExpression(DeviceProvisioningContract.IdentifierPattern)]
        public string GardenId { get; set; } = string.Empty;

        [StringLength(DeviceProvisioningContract.MaximumErrorCodeLength)]
        [RegularExpression(DeviceProvisioningContract.ErrorCodePattern)]
        public string ErrorCode { get; set; } = string.Empty;

        [Required, RegularExpression(DeviceProvisioningContract.HmacPattern)]
        public required string AuthenticationTag { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Decision == ClaimDecision.Approved)
            {
                if (string.IsNullOrWhiteSpace(UserId))
                    yield return new ValidationResult("UserId es obligatorio para un claim aprobado.", [nameof(UserId)]);
                if (string.IsNullOrWhiteSpace(GardenId))
                    yield return new ValidationResult("GardenId es obligatorio para un claim aprobado.", [nameof(GardenId)]);
                if (!string.IsNullOrEmpty(ErrorCode))
                    yield return new ValidationResult("ErrorCode debe estar vacío para un claim aprobado.", [nameof(ErrorCode)]);
            }
            else
            {
                if (!string.IsNullOrEmpty(UserId) || !string.IsNullOrEmpty(GardenId))
                    yield return new ValidationResult("UserId y GardenId deben estar vacíos para un claim no aprobado.", [nameof(UserId), nameof(GardenId)]);
                if (string.IsNullOrWhiteSpace(ErrorCode))
                    yield return new ValidationResult("ErrorCode es obligatorio para un claim rechazado o expirado.", [nameof(ErrorCode)]);
            }
        }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
