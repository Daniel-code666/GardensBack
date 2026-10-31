namespace GardensBack.Application.FirmwareReleases.Constants
{
    public static class FirmwareReleaseContract
    {
        public const int Version = 1;
        public const string ManifestType = "firmware_manifest";
        public const string ControllerRoute = "api/FirmwareReleases";
        public const string ManifestRoute = "{releaseId}/manifest";
    }
}
