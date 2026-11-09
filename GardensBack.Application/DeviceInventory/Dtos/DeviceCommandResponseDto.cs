using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceCommandResponseDto
    {
        public int Id { get; set; }
        public int MicrocontrollerDeviceId { get; set; }
        public required string DeviceId { get; set; }
        public required string RequestId { get; set; }
        public uint CommandVersion { get; set; }
        public DeviceCommandType Command { get; set; }
        public CommandStatus Status { get; set; }
        public bool? Success { get; set; }
        public DeviceActionResultMessage? DeviceResponse { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
