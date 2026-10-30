using GardensBack.Application.Common.Validation;
using System.ComponentModel.DataAnnotations;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    /// <summary>
    /// Clase para representar el mensaje que envía el esp al back para registrar la versión del firmware
    /// </summary>
    public class DeviceInventoryMessage
    {
        [Required]
        [RegularExpression("^device_inventory$")]
        public required string Type { get; set; }

        [Range(1, 1)]
        public int ContractVersion { get; set; }

        [Required]
        [StringLength(64)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$")]
        public required string DeviceId { get; set; }

        [Required]
        [StringLength(32)]
        [RegularExpression(@"^\d+\.\d+\.\d+(?:[-+][A-Za-z0-9.-]+)?$")]
        public required string FirmwareVersion { get; set; }

        [Required]
        [MinLength(1)]
        public required IReadOnlyList<DeviceInventoryItemMessage> Devices { get; set; }

        public void Validate()
            => ValidationUtilities.ValidateObject(this);
    }
}
