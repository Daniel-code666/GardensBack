using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(DeviceId), nameof(CreationDate))]
    [Table("DeviceTelemetry", Schema = "Data")]
    public class DeviceTelemetry : BaseEntity
    {
        // propiedades
        [Key]
        public int Id { get; set; }
        public float Value { get; set; }
        public TelemetrySource Source { get; set; }
        public string? RequestId { get; set; }
        public ActuatorMode? ActuatorMode { get; set; }
        public bool? State { get; set; }

        // relaciones
        [ForeignKey(nameof(Device))]
        public int DeviceId { get; set; }
        public required Device Device { get; set; }
    }
}
