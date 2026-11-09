namespace GardensBack.Application.DeviceInventory.Configuration
{
    public class DeviceCommandExpirationOptions
    {
        public const string SectionName = "DeviceCommandExpiration";

        public int SweepIntervalSeconds { get; set; } = 5;
    }
}
