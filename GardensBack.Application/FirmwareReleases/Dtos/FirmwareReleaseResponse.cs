using GardensBack.Domain.Enums;

namespace GardensBack.Application.FirmwareReleases.Dtos
{
    public class FirmwareReleaseResponse
    {
        public required string ReleaseId { get; set; }
        public required string Version { get; set; }
        public required string Board { get; set; }
        public long SizeBytes { get; set; }
        public required string Sha256 { get; set; }
        public required string FirmwareUrl { get; set; }
        public required string Signature { get; set; }
        public int ContractVersion { get; set; }
        public FirmwareReleaseStatus Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool Active { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
