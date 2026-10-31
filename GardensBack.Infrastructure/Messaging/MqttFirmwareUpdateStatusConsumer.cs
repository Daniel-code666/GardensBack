using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.Common.Messaging;
using GardensBack.Application.DeviceProvisioning.Configuration;
using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Application.FirmwareUpdates.Dtos;
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
    public class MqttFirmwareUpdateStatusConsumer(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options, ILogger<MqttFirmwareUpdateStatusConsumer> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly RabbitMqOptions _rabbitMqOptions = options.Value;
        private readonly ILogger<MqttFirmwareUpdateStatusConsumer> _logger = logger;
        private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

        private async Task ProcessMessage(MqttApplicationMessageReceivedEventArgs args, CancellationToken ct)
        {
            try
            {
                string topic = args.ApplicationMessage.Topic;
                if (!TryGetDeviceId(topic, out string deviceId))
                {
                    _logger.LogWarning("Se descartó un tópico de estado OTA inválido: {Topic}.", topic);
                    return;
                }

                ReadOnlySequence<byte> mqttPayload = args.ApplicationMessage.Payload;
                if (mqttPayload.IsEmpty || mqttPayload.Length > 4096)
                {
                    _logger.LogWarning("Se descartó el estado OTA de {DeviceId} por tener un tamaño inválido.", deviceId);
                    return;
                }

                OtaUpdateStatusMessage? message = JsonSerializer.Deserialize<OtaUpdateStatusMessage>(mqttPayload.ToArray(), JsonOptions);
                if (message is null)
                {
                    _logger.LogWarning("No fue posible deserializar el estado OTA de {DeviceId}.", deviceId);
                    return;
                }

                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
                IDeviceFirmwareUpdateStatusService service = scope.ServiceProvider.GetRequiredService<IDeviceFirmwareUpdateStatusService>();
                await service.ProcessStatus(deviceId, message, ct);
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Se recibió un estado OTA con JSON inválido.");
            }
            catch (AppException exception)
            {
                _logger.LogWarning("Se descartó un estado OTA: {Message}", exception.Message);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // La aplicación se está cerrando.
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ocurrió un error procesando un estado OTA.");
            }
        }

        private static bool TryGetDeviceId(string topic, out string deviceId)
        {
            deviceId = string.Empty;

            if (string.IsNullOrWhiteSpace(topic))
                return false;

            string[] segments = topic.Split('/', StringSplitOptions.None);
            if (segments.Length != 3 ||
                !segments[0].Equals(DeviceMqttTopics.Root, StringComparison.Ordinal) ||
                !segments[2].Equals(DeviceMqttTopics.ControlStatus, StringComparison.Ordinal) ||
                !DeviceMqttTopics.IsValidDeviceId(segments[1]))
            {
                return false;
            }

            deviceId = segments[1];
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
            TimeSpan reconnectDelay = TimeSpan.FromSeconds(5);
            bool subscribed = false;
            MqttClientFactory factory = new();
            using IMqttClient mqttClient = factory.CreateMqttClient();

            mqttClient.ApplicationMessageReceivedAsync += args => ProcessMessage(args, ct);
            mqttClient.DisconnectedAsync += args =>
            {
                subscribed = false;
                if (!ct.IsCancellationRequested)
                    _logger.LogWarning("El consumidor de estados OTA perdió la conexión. Motivo: {Reason}.", args.Reason);
                return Task.CompletedTask;
            };

            MqttClientOptions clientOptions = new MqttClientOptionsBuilder()
                .WithClientId(_rabbitMqOptions.FirmwareStatusConsumerClientId)
                .WithTcpServer(_rabbitMqOptions.Host, _rabbitMqOptions.MqttPort)
                .WithCredentials(_rabbitMqOptions.Username, _rabbitMqOptions.Password)
                .WithCleanSession()
                .Build();

            MqttClientSubscribeOptions subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(filter => filter.WithTopic(DeviceMqttTopics.ControlStatusSubscription).WithAtMostOnceQoS())
                .Build();

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (!mqttClient.IsConnected)
                    {
                        subscribed = false;
                        await mqttClient.ConnectAsync(clientOptions, ct);
                        _logger.LogInformation("Consumidor de estados OTA conectado al broker.");
                    }

                    if (!subscribed)
                    {
                        await mqttClient.SubscribeAsync(subscribeOptions, ct);
                        subscribed = true;
                        _logger.LogInformation("Consumidor de estados OTA suscrito a {Topic}.", DeviceMqttTopics.ControlStatusSubscription);
                    }

                    await Task.Delay(reconnectDelay, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "No fue posible conectar o mantener el consumidor de estados OTA.");
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
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "No fue posible cerrar ordenadamente el consumidor de estados OTA.");
            }
        }
    }
}
