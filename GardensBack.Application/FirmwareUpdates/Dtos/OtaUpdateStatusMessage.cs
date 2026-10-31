using GardensBack.Application.Common.Validation;
using GardensBack.Application.FirmwareUpdates.Constants;
using GardensBack.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GardensBack.Application.FirmwareUpdates.Dtos
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public class OtaUpdateStatusMessage : IValidatableObject
    {
        private static readonly Regex VersionPattern = new(@"^\d+\.\d+\.\d+$", RegexOptions.CultureInvariant);

        [Required]
        [RegularExpression("^" + FirmwareUpdateContract.UpdateStatusType + "$")]
        public required string Type { get; set; }

        [JsonRequired]
        [Range(FirmwareUpdateContract.Version, FirmwareUpdateContract.Version)]
        public int ContractVersion { get; set; }

        [Required, StringLength(FirmwareUpdateContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$")]
        public required string DeviceId { get; set; }

        [Required, StringLength(FirmwareUpdateContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$")]
        public required string RequestId { get; set; }

        [Required, StringLength(FirmwareUpdateContract.MaximumIdentifierLength, MinimumLength = 1)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$")]
        public required string ReleaseId { get; set; }

        [JsonRequired]
        public FirmwareUpdateStatus Status { get; set; }

        [JsonRequired]
        [Range(0, 100)]
        public int ProgressPercent { get; set; }

        [Required, StringLength(32, MinimumLength = 1)]
        [RegularExpression(@"^\d+\.\d+\.\d+$")]
        public required string CurrentVersion { get; set; }

        [StringLength(32)]
        public string? TargetVersion { get; set; }

        [StringLength(64)]
        public string? FailureCode { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Enum.IsDefined(Status))
                yield return new ValidationResult("El estado OTA no es válido.", [nameof(Status)]);

            if (!string.IsNullOrEmpty(TargetVersion) && !VersionPattern.IsMatch(TargetVersion))
                yield return new ValidationResult("TargetVersion no tiene un formato válido.", [nameof(TargetVersion)]);

            if ((Status is FirmwareUpdateStatus.Downloading or FirmwareUpdateStatus.PendingValidation or FirmwareUpdateStatus.Confirmed) && string.IsNullOrWhiteSpace(TargetVersion))
                yield return new ValidationResult("TargetVersion es obligatorio para el estado informado.", [nameof(TargetVersion)]);

            if (Status == FirmwareUpdateStatus.Failed && string.IsNullOrWhiteSpace(FailureCode))
                yield return new ValidationResult("FailureCode es obligatorio cuando el estado es Failed.", [nameof(FailureCode)]);

            if (Status != FirmwareUpdateStatus.Failed && !string.IsNullOrWhiteSpace(FailureCode))
                yield return new ValidationResult("FailureCode solo puede informarse cuando el estado es Failed.", [nameof(FailureCode)]);
        }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
