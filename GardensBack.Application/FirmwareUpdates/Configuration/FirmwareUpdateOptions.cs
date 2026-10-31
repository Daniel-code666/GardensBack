namespace GardensBack.Application.FirmwareUpdates.Configuration
{
    public class FirmwareUpdateOptions
    {
        public const string SectionName = "FirmwareUpdate";

        public int CommandExpirationMinutes { get; set; } = 5;
    }
}
