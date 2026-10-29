using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Table("Device", Schema = "Data")]
    public class Device : BaseEntity
    {
        // propiedades
        [Key]
        public int Id { get; set; }
        public int LocalDeviceId { get; set; }
        public required string Name { get; set; }
        public int? DevicePin { get; set; }
        public ConnectionType ConnectionType { get; set; }
        public MeasurementType? MeasurementType { get; set; }
        public MeasurementUnit? MeasurementUnit { get; set; }
        public DeviceType DeviceType { get; set; }

        // relaciones
        [ForeignKey(nameof(MicroCtrlDevice))]
        public int MicroCtrlDeviceId { get; set; }
        public required MicrocontrollerDevice MicroCtrlDevice { get; set; }

        public ICollection<DeviceTelemetry> DeviceTelemetries { get; set; } = [];
    }
}
