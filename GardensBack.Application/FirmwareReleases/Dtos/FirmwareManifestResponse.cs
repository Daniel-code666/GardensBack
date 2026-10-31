using GardensBack.Application.Common.Validation;
using GardensBack.Application.FirmwareReleases.Constants;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GardensBack.Application.FirmwareReleases.Dtos
{
    public class FirmwareManifestResponse
    {
        [Required]
        [RegularExpression("^firmware_manifest$")]
        [JsonPropertyName("type")]
        public string Type { get; set; } = FirmwareReleaseContract.ManifestType;

        [Range(1, 1)]
        [JsonPropertyName("contractVersion")]
        public int ContractVersion { get; set; }

        [Required]
        [StringLength(64)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$")]
        [JsonPropertyName("releaseId")]
        public required string ReleaseId { get; set; }

        [Required]
        [StringLength(32)]
        [RegularExpression(@"^\d+\.\d+\.\d+$")]
        [JsonPropertyName("version")]
        public required string Version { get; set; }

        [Required]
        [StringLength(64)]
        [JsonPropertyName("board")]
        public required string Board { get; set; }

        [Range(1, uint.MaxValue)]
        [JsonPropertyName("size")]
        public long Size { get; set; }

        [Required]
        [StringLength(64, MinimumLength = 64)]
        [RegularExpression(@"^[0-9a-f]{64}$")]
        [JsonPropertyName("sha256")]
        public required string Sha256 { get; set; }

        [Required]
        [StringLength(512)]
        [Url]
        [JsonPropertyName("firmwareUrl")]
        public required string FirmwareUrl { get; set; }

        [Required]
        [StringLength(160)]
        [RegularExpression(@"^(?:[0-9a-f]{2}){1,80}$")]
        [JsonPropertyName("signature")]
        public required string Signature { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
