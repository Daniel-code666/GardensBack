using GardensBack.Application.Common.Messaging;
using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Application.DeviceProvisioning.Configuration;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text.Json;

namespace GardensBack.Infrastructure.Messaging
{
    public class MqttDeviceActionPublisher(IOptions<RabbitMqOptions> options) : IDeviceActionPublisher
    {
        private readonly RabbitMqOptions _options = options.Value;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public async Task PublishCommand(string deviceId, DeviceActionMessage command, CancellationToken ct)
        {
            // armar el tópico al cual se va a enviar el comando
            string topic = DeviceMqttTopics.BuildActions(deviceId);
            // armar el payload del mensaje
            string payload = JsonSerializer.Serialize(command, JsonOptions);

            // instanciar objeto del cliente a través del factory
            MqttClientFactory factory = new();
            using IMqttClient client = factory.CreateMqttClient();

            // crear las optiones del cliente para la conexión tcp con el broker
            MqttClientOptions clientOptions = new MqttClientOptionsBuilder()
                .WithClientId($"gardens-back-action-{Guid.NewGuid():N}")
                .WithTcpServer(_options.Host, _options.MqttPort)
                .WithCredentials(_options.Username, _options.Password)
                .WithCleanSession()
                .Build();

            // crea el mensaje usando la clase indicada para .NET
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            // conecta el cliente y envía el mensaje
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
