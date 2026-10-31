using GardensBack.Domain.Enums;

namespace GardensBack.Application.FirmwareUpdates.Dtos
{
    public class DeviceFirmwareUpdateResponse
    {
        public int Id { get; set; }
        public required string RequestId { get; set; }
        public required string DeviceId { get; set; }
        public required string ReleaseId { get; set; }
        public required string PreviousVersion { get; set; }
        public required string TargetVersion { get; set; }
        public uint CommandVersion { get; set; }
        public FirmwareUpdateStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? PendingValidationAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? ProgressPercentage { get; set; }
        public string? FailureCode { get; set; }
        public string? FailureMessage { get; set; }
    }
}
