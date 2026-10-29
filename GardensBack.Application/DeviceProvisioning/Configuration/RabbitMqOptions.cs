namespace GardensBack.Application.DeviceProvisioning.Configuration
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMQ";

        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
