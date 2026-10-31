using GardensBack.Application.FirmwareUpdates.Dtos;

namespace GardensBack.Application.FirmwareUpdates.Abstractions
{
    public interface IOtaCommandAuthenticationService
    {
        string Sign(OtaUpdateCommand command, byte[] key);
    }
}
