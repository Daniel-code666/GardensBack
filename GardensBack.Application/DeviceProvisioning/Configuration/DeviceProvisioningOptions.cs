namespace GardensBack.Application.DeviceProvisioning.Configuration
{
    public class DeviceProvisioningOptions
    {
        public const string SectionName = "DeviceProvisioning";
        public int TokenExpirationMinutes { get; set; } = 10;
    }
}
