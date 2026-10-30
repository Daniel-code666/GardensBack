namespace GardensBack.Application.DeviceProvisioning.Configuration
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMQ";
        public required string Host { get; set; }
        public int MqttPort { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string InventoryConsumerClientId { get; set; }
    }
}
