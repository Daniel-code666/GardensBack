namespace GardensBack.Application.FirmwareUpdates.Abstractions
{
    public interface IDeviceCommandKeyProtector
    {
        string Protect(string keyHex);
        byte[] Unprotect(string protectedKey);
    }
}
