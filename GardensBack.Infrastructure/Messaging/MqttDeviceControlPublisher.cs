using GardensBack.Application.Common.Messaging;
using GardensBack.Application.DeviceProvisioning.Configuration;
using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Application.FirmwareUpdates.Dtos;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text.Json;

namespace GardensBack.Infrastructure.Messaging
{
    public class MqttDeviceControlPublisher(IOptions<RabbitMqOptions> options) : IDeviceControlPublisher
    {
        private readonly RabbitMqOptions _options = options.Value;
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task Publish(string deviceId, OtaUpdateCommand command, CancellationToken ct)
        {
            string topic = DeviceMqttTopics.BuildControl(deviceId);
            string payload = JsonSerializer.Serialize(command, JsonOptions);
            MqttClientFactory factory = new();
            using IMqttClient client = factory.CreateMqttClient();

            MqttClientOptions clientOptions = new MqttClientOptionsBuilder()
                .WithClientId($"gardens-back-control-{Guid.NewGuid():N}")
                .WithTcpServer(_options.Host, _options.MqttPort)
                .WithCredentials(_options.Username, _options.Password)
                .WithCleanSession()
                .Build();

            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await client.ConnectAsync(clientOptions, ct);
            MqttClientPublishResult result = await client.PublishAsync(message, ct);

            if (!result.IsSuccess)
                throw new InvalidOperationException($"RabbitMQ rechazó: {result.ReasonCode}");

            if (client.IsConnected)
            {
                MqttClientDisconnectOptions disconnectOptions = factory.CreateClientDisconnectOptionsBuilder().Build();
                await client.DisconnectAsync(disconnectOptions, CancellationToken.None);
            }
        }
    }
}
