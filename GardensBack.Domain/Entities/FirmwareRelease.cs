using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(ReleaseId), IsUnique = true)]
    [Index(nameof(Board), nameof(Version), IsUnique =  true)]
    [Table("FirmwareRelease", Schema = "Data")]
    public class FirmwareRelease : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public required string ReleaseId { get; set; }
        public required string Version { get; set; }
        public required string Board { get; set; }
        public required long SizeBytes { get; set; }
        public required string Sha256 { get; set; }
        public required string FirmwareUrl { get; set; }
        public required string Signature { get; set; }
        public required int ContractVersion { get; set; }
        public FirmwareReleaseStatus Status { get; set; }
        public DateTime? PublishedAt { get; set; }

        // relaciones
        public ICollection<DeviceFirmwareUpdate> DeviceFirmwareUpdates { get; set; } = [];
    }
}
