namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceCommandExecutionResponseDto
    {
        public required DeviceCommandResponseDto Command { get; set; }
        public DeviceActionResultMessage? DeviceResponse { get; set; }
        public bool Responded => DeviceResponse is not null;
    }
}
