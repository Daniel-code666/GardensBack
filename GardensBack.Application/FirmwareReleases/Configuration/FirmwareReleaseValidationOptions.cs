namespace GardensBack.Application.FirmwareReleases.Configuration
{
    public class FirmwareReleaseValidationOptions
    {
        public const string SectionName = "FirmwareReleaseValidation";

        public required string PublicKeyPem { get; set; }
        public int DownloadTimeoutSeconds { get; set; } = 30;
        public long MaximumFirmwareSizeBytes { get; set; } = 16 * 1024 * 1024;
        public bool AllowHttp { get; set; }
    }
}
