using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(MicroCtrlDeviceId), nameof(RequestId), IsUnique = true)]
    [Index(nameof(Status), nameof(ExpiresAt))]
    [Table("DeviceCommand", Schema = "Data")]
    public class DeviceCommand : BaseEntity
    {
        // propiedades
        [Key]
        public int Id { get; set; }
        public required string Command { get; set; }
        public required string RequestId { get; set; }
        public string? Payload { get; set; }
        public CommandStatus Status { get; set; }
        public bool? Success { get; set; }
        public string? ResponsePayload { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // relaciones
        [ForeignKey(nameof(MicroCtrlDevice))]
        public int MicroCtrlDeviceId { get; set; }
        public required MicrocontrollerDevice MicroCtrlDevice { get; set; }
    }
}
