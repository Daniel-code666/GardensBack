using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardensBack.Domain.Entities
{
    [Index(nameof(ConfigurationKey), IsUnique = true)]
    [Table("DeviceConnectivityConfiguration", Schema = "Data")]
    public class DeviceConnectivityConfiguration : BaseEntity
    {
        public const string DefaultConfigurationKey = "default";

        // propiedades
        [Key]
        public int Id { get; set; }
        [Required, StringLength(32)]
        public string ConfigurationKey { get; set; } = DefaultConfigurationKey;
        [Required, StringLength(253)]
        public required string ApiHost { get; set; }
        [Range(1, 65535)]
        public int ApiPort { get; set; }
        public bool ApiUseTls { get; set; }
        [Required, StringLength(253)]
        public required string MqttHost { get; set; }
        [Range(1, 65535)]
        public int MqttPort { get; set; }
        public bool MqttUseTls { get; set; }
    }
}
