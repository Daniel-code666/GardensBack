namespace GardensBack.Application.DeviceConnectivity.Dtos
{
    public class DeviceConnectivityConfigurationResponse
    {
        public int Id { get; set; }
        public required string ApiHost { get; set; }
        public int ApiPort { get; set; }
        public bool ApiUseTls { get; set; }
        public required string ApiBaseUrl { get; set; }
        public required string MqttHost { get; set; }
        public int MqttPort { get; set; }
        public bool MqttUseTls { get; set; }
        public required string MqttEndpoint { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
