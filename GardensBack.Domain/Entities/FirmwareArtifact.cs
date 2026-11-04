using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(FirmwareReleaseId), IsUnique = true)]
    [Table("FirmwareArtifact", Schema = "Data")]
    public class FirmwareArtifact
    {
        // propiedades
        [Key]
        public int Id { get; set; }
        [Required]
        public required byte[] Content { get; set; }
        [Required]
        [StringLength(100)]
        public string FileName { get; set; } = "firmware.bin";

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = "application/octet-stream";

        // relaciones
        [ForeignKey(nameof(FirmwareRelease))]
        public int FirmwareReleaseId { get; set; }

        public required FirmwareRelease FirmwareRelease { get; set; }
    }
}
