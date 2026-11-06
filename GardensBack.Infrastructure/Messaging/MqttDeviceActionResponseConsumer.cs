using GardensBack.Application.Common.Exceptions;
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
    public class MqttDeviceActionResponseConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<MqttDeviceActionResponseConsumer> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly RabbitMqOptions _rabbitMqOptions = options.Value;
        private readonly ILogger<MqttDeviceActionResponseConsumer> _logger = logger;
        private readonly SemaphoreSlim _processingLock = new(1, 1);
        private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

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
                    _logger.LogWarning(
                        "El consumidor de respuestas de acciones perdió la conexión. Motivo: {Reason}.",
                        args.Reason);
                return Task.CompletedTask;
            };

            MqttClientOptions clientOptions = new MqttClientOptionsBuilder()
                .WithClientId(_rabbitMqOptions.ActionResponseConsumerClientId)
                .WithTcpServer(_rabbitMqOptions.Host, _rabbitMqOptions.MqttPort)
                .WithCredentials(_rabbitMqOptions.Username, _rabbitMqOptions.Password)
                .WithCleanSession()
                .Build();

            MqttClientSubscribeOptions subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(filter => filter
                    .WithTopic(DeviceMqttTopics.ActionsResponseSubscription)
                    .WithAtMostOnceQoS())
                .Build();

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (!mqttClient.IsConnected)
                    {
                        subscribed = false;
                        await mqttClient.ConnectAsync(clientOptions, ct);
                        _logger.LogInformation("Consumidor de respuestas de acciones conectado al broker.");
                    }

                    if (!subscribed)
                    {
                        await mqttClient.SubscribeAsync(subscribeOptions, ct);
                        subscribed = true;
                        _logger.LogInformation(
                            "Consumidor de respuestas de acciones suscrito a {Topic}.",
                            DeviceMqttTopics.ActionsResponseSubscription);
                    }

                    await Task.Delay(reconnectDelay, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "No fue posible conectar o mantener el consumidor de respuestas de acciones.");
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
                MqttClientDisconnectOptions disconnectOptions = factory
                    .CreateClientDisconnectOptionsBuilder()
                    .Build();
                await mqttClient.DisconnectAsync(disconnectOptions, CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "No fue posible cerrar ordenadamente el consumidor de respuestas de acciones.");
            }
        }

        private async Task ProcessMessage(MqttApplicationMessageReceivedEventArgs args, CancellationToken ct)
        {
            await _processingLock.WaitAsync(ct);
            try
            {
                string topic = args.ApplicationMessage.Topic;
                if (!TryGetDeviceId(topic, out string deviceId))
                {
                    _logger.LogWarning("Se descartó un tópico de respuesta de acción inválido: {Topic}.", topic);
                    return;
                }

                ReadOnlySequence<byte> payload = args.ApplicationMessage.Payload;
                if (payload.IsEmpty || payload.Length > 4096)
                {
                    _logger.LogWarning(
                        "Se descartó la respuesta de acción de {DeviceId} por tener un tamaño inválido.",
                        deviceId);
                    return;
                }

                DeviceActionResultMessage? response = JsonSerializer.Deserialize<DeviceActionResultMessage>(
                    payload.ToArray(),
                    JsonOptions);
                if (response is null)
                {
                    _logger.LogWarning(
                        "No fue posible deserializar la respuesta de acción de {DeviceId}.",
                        deviceId);
                    return;
                }

                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
                IDeviceInventoryService service = scope.ServiceProvider.GetRequiredService<IDeviceInventoryService>();
                await service.ProcessDeviceCommandResponse(deviceId, response, ct);
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Se recibió una respuesta de acción con JSON inválido.");
            }
            catch (AppException exception)
            {
                _logger.LogWarning("Se descartó una respuesta de acción: {Message}", exception.Message);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // La aplicación se está cerrando.
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ocurrió un error procesando una respuesta de acción.");
            }
            finally
            {
                _processingLock.Release();
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
                !segments[2].Equals(DeviceMqttTopics.ActionsResponse, StringComparison.Ordinal) ||
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

        public override void Dispose()
        {
            _processingLock.Dispose();
            base.Dispose();
        }
    }
}
