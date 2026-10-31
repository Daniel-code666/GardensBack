using GardensBack.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(RequestId), IsUnique = true)]
    [Index(nameof(Version))]
    [Index(nameof(MicrocontrollerDeviceId), IsUnique = true)]
    [Table("MqttCredential", Schema = "Data")]
    public class MqttCredential : BaseEntity
    {
        // propiedades
        [Key]
        public int Id { get; set; }
        public int Version { get; set; }
        public string? RequestId { get; set; }
        [Required]
        public required string UserName { get; set; }
        public string? MqttClientId { get; set; }
        public required string Host { get; set; }
        public int Port { get; set; }
        public required string SecretReference { get; set; }
        public MqttCredentialStatus Status { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? FailureCode { get; set; }
        [StringLength(24)]
        public string? ResponseNonce { get; set; }
        [StringLength(4096)]
        public string? ResponseCiphertext { get; set; }
        [StringLength(32)]
        public string? ResponseAuthenticationTag { get; set; }
        [StringLength(256)]
        public string? ProtectedCommandKey { get; set; }

        // relaciones
        [ForeignKey(nameof(MicroCtrlDevice))]
        public int MicrocontrollerDeviceId { get; set; }
        public required MicrocontrollerDevice MicroCtrlDevice { get; set; }
    }
}
