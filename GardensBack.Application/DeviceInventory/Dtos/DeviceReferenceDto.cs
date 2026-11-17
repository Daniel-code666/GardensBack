using GardensBack.Domain.Enums;

namespace GardensBack.Application.DeviceInventory.Dtos
{
    public class DeviceReferenceDto
    {
        public int Id { get; set; }
        public DeviceType DeviceType { get; set; }
    }
}
