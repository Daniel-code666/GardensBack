namespace GardensBack.Application.FirmwareUpdates.Configuration
{
    public class DeviceCommandSecurityOptions
    {
        public const string SectionName = "DeviceCommandSecurity";

        public required string EncryptionKey { get; set; }
    }
}
