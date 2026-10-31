namespace GardensBack.Application.FirmwareUpdates.Constants
{
    public static class FirmwareUpdateContract
    {
        public const int Version = 1;
        public const string UpdateRequestType = "ota_update_request";
        public const string UpdateStatusType = "ota_update_status";
        public const int MaximumIdentifierLength = 64;
    }
}
