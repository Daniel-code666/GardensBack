using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class MicroControllerDeviceDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; } = string.Empty;
        public required string PublicId { get; set; }
        public required string HardwareId { get; set; }
        public required string MqttClientId { get; set; }
        public required string Board { get; set; }
        public required string FirmwareVersion { get; set; }
        public required ProvisioningState ProvisioningState { get; set; }
        public required string DeviceId { get; set; }
        public int? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public string? OwnerEmail { get; set; }
        public required int ExistingDevicesQty { get; set; }
        public required DateTime CreationDate { get; set; }
        public required DateTime UpdateDate { get; set; }
        public required bool Active { get; set; }
        public int[] Devices { get; set; } = [];
    }
}
