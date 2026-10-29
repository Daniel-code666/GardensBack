using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(RoleId))]
    [Table("User", Schema = "Data")]
    public class User : BaseEntity
    {
        // propiedades
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = "";
        [EmailAddress]
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public int TokenVersion { get; set; }

        // relaciones
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        public required Role Role { get; set; }

        public ICollection<MicrocontrollerDevice> MicrocontrollerDevices { get; set; } = [];

        public ICollection<DeviceClaim> ResolvedDeviceClaims { get; set; } = [];

        public ICollection<DeviceFirmwareUpdate> RequestedFirmwareUpdates { get; set; } = [];
    }
}
