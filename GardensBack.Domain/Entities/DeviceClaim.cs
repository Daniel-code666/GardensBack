using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(RequestId), IsUnique = true)]
    [Index(nameof(MicrocontrollerDeviceId), IsUnique = true)]
    [Index(nameof(DeviceClaimStatus), nameof(ExpiresAt))]
    [Index(nameof(ResolvedByUserId))]
    [Index(nameof(BootstrapRequestId), IsUnique = true)]
    [Index(nameof(ProvisioningTokenHash), IsUnique = true)]
    [Table("DeviceClaim", Schema = "Data")]
    public class DeviceClaim : BaseEntity
    {
        // propiedades
        [Key]
        public int Id { get; set; }
        public string? RequestId { get; set; }
        public string? ClaimCodeHash { get; set; }
        public int ContractVersion { get; set; }
        public DeviceClaimStatus DeviceClaimStatus { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ErrorCode { get; set; } 
        public int AttemptCount { get; set; }
        [StringLength(64)]
        public string? BootstrapRequestId { get; set; }
        [StringLength(64)]
        public string? ProvisioningTokenHash { get; set; }
        public DateTime? ProvisioningTokenExpiresAt { get; set; }
        public DateTime? ProvisioningTokenConsumedAt { get; set; }
        [StringLength(64)]
        public string? ResponseAuthenticationTag { get; set; }

        // relaciones
        [ForeignKey(nameof(MicroCtrlDevice))]
        public int MicrocontrollerDeviceId { get; set; }
        public required MicrocontrollerDevice MicroCtrlDevice { get; set; }

        [ForeignKey(nameof(ResolvedByUser))]
        public int? ResolvedByUserId { get; set; }
        public User? ResolvedByUser { get; set; }
    }
}
