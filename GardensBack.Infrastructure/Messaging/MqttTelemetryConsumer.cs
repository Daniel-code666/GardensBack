using GardensBack.Application.Common.Messaging;
using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
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
    public class MqttTelemetryConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options, ILogger<MqttTelemetryConsumer> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly RabbitMqOptions _rabbitMqOptions = options.Value;
        private readonly ILogger<MqttTelemetryConsumer> _logger = logger;
        private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            const string telemetryTopic = DeviceMqttTopics.TelemetrySubscription;
            TimeSpan reconnectDelay = TimeSpan.FromSeconds(5);
            bool subscribed = false;

            MqttClientFactory factory = new();

            using IMqttClient mqttClient = factory.CreateMqttClient();

            mqttClient.ApplicationMessageReceivedAsync += args => ProcessMessage(args, ct);

            mqttClient.DisconnectedAsync += args =>
            {
                subscribed = false;
                if (!ct.IsCancellationRequested)
                    _logger.LogWarning("El consumidor MQTT, motivo: {reason}", args.Reason);

                return Task.CompletedTask;
            };

            MqttClientOptions clientOptions = new MqttClientOptionsBuilder()
                .WithClientId(_rabbitMqOptions.TelemetryConsumerClientId)
                .WithTcpServer(_rabbitMqOptions.Host, _rabbitMqOptions.MqttPort)
                .WithCredentials(_rabbitMqOptions.Username, _rabbitMqOptions.Password)
                .WithCleanSession().Build();

            MqttClientSubscribeOptions subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(telemetryTopic).WithAtMostOnceQoS())
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
                        await mqttClient.SubscribeAsync(subscribeOptions, ct);

                        subscribed = true;

                        _logger.LogInformation("Consumidor MQTT suscrito a {Topic}.", telemetryTopic);
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

        private async Task ProcessMessage(MqttApplicationMessageReceivedEventArgs args, CancellationToken ct)
        {
            try
            {
                // extraer topic
                string topic = args.ApplicationMessage.Topic;

                // se intenta obtener el id del dispositivo a través del topic
                if (!TryGetDeviceId(topic, out string deviceId))
                {
                    _logger.LogWarning("Se descartó tópico MQTT: {topic}", topic);
                    return;
                }

                // se extrae el payload del mensaje
                ReadOnlySequence<byte> mqttPayload = args.ApplicationMessage.Payload;

                if (mqttPayload.IsEmpty)
                {
                    _logger.LogWarning("Se recibió telemetría vacía para {DeviceId}.", deviceId);
                    return;
                }

                // TODO validar el tamaño del payload

                byte[] payload = mqttPayload.ToArray();

                // deserializa el payload
                DeviceTelemetryMessage? telemetryMessage = JsonSerializer.Deserialize<DeviceTelemetryMessage>(payload, JsonOptions);

                if (telemetryMessage is null)
                {
                    _logger.LogWarning("No fue posible deserializar la telemetría de {DeviceId}.", deviceId);
                    return;
                }

                // crea el scope para llamar al servicio que procesa la telemetría
                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
                IDeviceInventoryService service = scope.ServiceProvider.GetRequiredService<IDeviceInventoryService>();
                await service.ProcessTelemetry(deviceId, telemetryMessage, ct);
            }
            catch(JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx, "Se recibió telemetría con JSON inválido.");
            }
            catch(OperationCanceledException) when (ct.IsCancellationRequested)
            {

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error procesando telemetría MQTT.");
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

            if (!segments[2].Equals(DeviceMqttTopics.Telemetry, StringComparison.Ordinal))
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
    }
}
