using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(PublicId), IsUnique=true)]
    [Index(nameof(HardwareId), IsUnique = true)]
    [Index(nameof(DeviceId), IsUnique = true)]
    [Index(nameof(MqttClientId), IsUnique = true)]
    [Index(nameof(UserId))]
    [Table("MicrocontrollerDevice", Schema = "Data")]
    public class MicrocontrollerDevice : BaseEntity
    {
        // propiedades
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Description { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid PublicId { get; set; } = Guid.NewGuid();
        public string? HardwareId { get; set; }
        public string? MqttClientId { get; set; }
        public string? Board { get; set; }
        public string? FirmwareVersion { get; set; }
        public ProvisioningState ProvisioningState { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public DateTime? UnclaimedAt { get; set; }
        public int LastRecoveryCommandVersion { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceIp { get; set; }
        public string? DeviceSSID { get; set; }

        // relaciones
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public required User User { get; set; }

        public DeviceClaim? DeviceClaim { get; set; }

        public MqttCredential? MqttCredential { get; set; }

        public ICollection<Device> Devices { get; set; } = [];

        public ICollection<DeviceCommand> DeviceCommands { get; set; } = [];

        public ICollection<DeviceFirmwareUpdate> DeviceFirmwareUpdates { get; set; } = [];
    }
}
