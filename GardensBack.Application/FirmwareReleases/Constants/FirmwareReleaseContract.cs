namespace GardensBack.Application.FirmwareReleases.Constants
{
    public static class FirmwareReleaseContract
    {
        public const int Version = 2;
        public const string ManifestType = "firmware_manifest";
        public const string ControllerRoute = "api/FirmwareReleases";
        public const string ManifestRoute = "{releaseId}/manifest";
        public const string ArtifactRoute = "{releaseId}/artifact";
    }
}
