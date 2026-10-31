using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(MicroCtlDeviceId), nameof(RequestId), IsUnique = true)]
    [Index(nameof(MicroCtlDeviceId), nameof(CreationDate))]
    [Index(nameof(FirmwareReleaseId))]
    [Index(nameof(Status), nameof(CreationDate))]
    [Index(nameof(MicroCtlDeviceId), nameof(CommandVersion), IsUnique = true)]
    [Index(nameof(RequestedByUserId))]
    [Table("DeviceFirmwareUpdate", Schema = "Data")]
    public class DeviceFirmwareUpdate : BaseEntity
    {
        // propiedades
        [Key]
        public int Id { get; set; }
        public required string RequestId { get; set; }
        public FirmwareUpdateStatus Status { get; set; }
        public required string PreviousVersion { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? PendingValidationAt { get; set; }
        public DateTime? DeviceConfirmedAt { get; set; }
        public DateTime? InventoryConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        [Range(0, 100, ErrorMessage = "El valor debe estar entre 0 y 100.")]
        public int? ProgressPercentage { get; set; }
        public string? FailureCode { get; set; }
        public string? FailureMessage { get; set; }
        public uint CommandVersion { get; set; }
        public DateTime ExpiresAt { get; set; }
        // relaciones
        [ForeignKey(nameof(MicrocontrollerDevice))]
        public int MicroCtlDeviceId { get; set; }
        public required MicrocontrollerDevice MicrocontrollerDevice { get; set; }

        [ForeignKey(nameof(FirmwareRelease))]
        public int FirmwareReleaseId { get; set; }
        public required FirmwareRelease FirmwareRelease { get; set; }

        [ForeignKey(nameof(RequestedByUser))]
        public int RequestedByUserId { get; set; }
        public required User RequestedByUser { get; set; }
    }
}
