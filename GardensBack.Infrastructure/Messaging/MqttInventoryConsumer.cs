using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Application.Common.Messaging;
using GardensBack.Application.DeviceProvisioning.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GardensBack.Infrastructure.Messaging
{
    public class MqttInventoryConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options, ILogger<MqttInventoryConsumer> logger) 
        : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly RabbitMqOptions _rabbitMqOptions = options.Value;
        private readonly ILogger<MqttInventoryConsumer> _logger = logger;
        private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

        private async Task ProcessMessage(MqttApplicationMessageReceivedEventArgs args, CancellationToken ct)
        {
            try
            {
                string topic = args.ApplicationMessage.Topic;


                if (!TryGetDeviceId(topic, out string deviceId))
                {
                    _logger.LogWarning("Se descartó tópico MQTT: {topic}", topic);
                    return;
                }

                ReadOnlySequence<byte> mqttPayload = args.ApplicationMessage.Payload;

                if (mqttPayload.IsEmpty)
                {
                    _logger.LogWarning("Se recibió un inventario vacío para {DeviceId}.", deviceId);
                    return;
                }

                if (mqttPayload.Length > 4096)
                {
                    _logger.LogWarning(
                        "Se descartó el inventario de {DeviceId} porque supera el tamaño permitido.",
                        deviceId);

                    return;
                }

                byte[] payload = mqttPayload.ToArray();

                DeviceInventoryMessage? inventory = JsonSerializer.Deserialize<DeviceInventoryMessage>(payload, JsonOptions);

                if (inventory is null)
                {
                    _logger.LogWarning("No fue posible deserializar el inventario de {DeviceId}.", deviceId);
                    return;
                }

                if (!inventory.DeviceId.Equals(deviceId, StringComparison.Ordinal))
                {
                    _logger.LogWarning(
                        "El DeviceId del tópico {TopicDeviceId} no coincide con el payload {PayloadDeviceId}.",
                        deviceId,
                        inventory.DeviceId);

                    return;
                }

                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();

                IDeviceInventoryService service = scope.ServiceProvider.GetRequiredService<IDeviceInventoryService>();

                await service.ProcessInventory(inventory, ct);
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Se recibió un inventario MQTT con JSON inválido.");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // La aplicación se está cerrando.
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ocurrió un error procesando un inventario MQTT.");
            }
        }

        private static bool TryGetDeviceId(string topic, out string deviceId)
        {
            deviceId = string.Empty;

            if (string.IsNullOrWhiteSpace(topic))
                return false;

            string[] segments = topic.Split('/', StringSplitOptions.None);

            if (segments.Length != 3)
                return false;

            if (!segments[0].Equals(DeviceMqttTopics.Root, StringComparison.Ordinal))
                return false;

            if (!segments[2].Equals(DeviceMqttTopics.Inventory, StringComparison.Ordinal))
                return false;

            string candidate = segments[1];

            if (!DeviceMqttTopics.IsValidDeviceId(candidate))
                return false;

            deviceId = candidate;
            return true;
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new JsonStringEnumConverter());

            return options;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            const string inventoryTopic = DeviceMqttTopics.InventorySubscription;
            TimeSpan reconnectDelay = TimeSpan.FromSeconds(5);
            bool subscribed = false;

            MqttClientFactory factory = new();

            using IMqttClient mqttClient = factory.CreateMqttClient();

            mqttClient.ApplicationMessageReceivedAsync += args => ProcessMessage(args, ct);

            mqttClient.DisconnectedAsync += args =>
            {
                subscribed = false;

                if (!ct.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "El consumidor MQTT perdió la conexión con el broker. Motivo: {Reason}.",
                        args.Reason);
                }

                return Task.CompletedTask;
            };

            MqttClientOptions clientOptions = new MqttClientOptionsBuilder()
                .WithClientId(_rabbitMqOptions.InventoryConsumerClientId)
                .WithTcpServer(_rabbitMqOptions.Host, _rabbitMqOptions.MqttPort)
                .WithCredentials(_rabbitMqOptions.Username, _rabbitMqOptions.Password)
                .WithCleanSession()
                .Build();

            MqttClientSubscribeOptions subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(inventoryTopic).WithAtMostOnceQoS())
                .Build();

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (!mqttClient.IsConnected)
                    {
                        subscribed = false;

                        _logger.LogInformation("Conectando el consumidor MQTT a {Host}:{Port}", _rabbitMqOptions.Host, _rabbitMqOptions.MqttPort);

                        await mqttClient.ConnectAsync(clientOptions, ct);

                        _logger.LogInformation("Consumidor MQTT conectado al broker.");
                    }

                    if (!subscribed)
                    {
                        await mqttClient.SubscribeAsync(
                            subscribeOptions,
                            ct);

                        subscribed = true;

                        _logger.LogInformation("Consumidor MQTT suscrito a {Topic}.", inventoryTopic);
                    }

                    await Task.Delay(reconnectDelay, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "No fue posible conectar o mantener el consumidor MQTT. Se reintentará en {Seconds} segundos.", reconnectDelay.TotalSeconds);

                    try
                    {
                        await Task.Delay(reconnectDelay, ct);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }

            if (!mqttClient.IsConnected)
                return;

            try
            {
                MqttClientDisconnectOptions disconnectOptions = factory.CreateClientDisconnectOptionsBuilder().Build();

                await mqttClient.DisconnectAsync(disconnectOptions, CancellationToken.None);

                _logger.LogInformation("Consumidor MQTT desconectado correctamente.");
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "No fue posible cerrar ordenadamente la conexión MQTT.");
            }
        }
    }
}
