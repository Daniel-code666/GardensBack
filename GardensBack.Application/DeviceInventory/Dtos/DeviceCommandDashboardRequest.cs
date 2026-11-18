using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceCommandDashboardRequest
    {
        public int MicrocrontrollerId { get; set; }
        public int ItemPerRequest { get; set; }
        public CommandStatus[] StatusList { get; set; } = [];
    }
}
