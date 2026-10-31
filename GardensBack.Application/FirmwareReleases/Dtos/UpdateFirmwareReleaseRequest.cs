using GardensBack.Application.Common.Validation;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.FirmwareReleases.Dtos
{
    public class UpdateFirmwareReleaseRequest
    {
        [Required]
        [StringLength(64)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$")]
        public required string ReleaseId { get; set; }

        [Required]
        [StringLength(32)]
        [RegularExpression(@"^\d+\.\d+\.\d+(?:[-+][A-Za-z0-9.-]+)?$")]
        public required string Version { get; set; }

        [Required]
        [StringLength(64)]
        public required string Board { get; set; }

        [Range(1, uint.MaxValue)]
        public long SizeBytes { get; set; }

        [Required]
        [StringLength(64, MinimumLength = 64)]
        [RegularExpression(@"^[0-9a-f]{64}$")]
        public required string Sha256 { get; set; }

        [Required]
        [StringLength(512)]
        [Url]
        public required string FirmwareUrl { get; set; }

        [Required]
        [StringLength(160)]
        [RegularExpression(@"^(?:[0-9a-f]{2}){1,80}$")]
        public required string Signature { get; set; }

        [Range(1, 1)]
        public int ContractVersion { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
