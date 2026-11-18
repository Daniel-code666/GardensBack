using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using Microsoft.AspNetCore.SignalR;
using System.Security.Cryptography.X509Certificates;

namespace GardensBack.Realtime
{
    /// <summary>
    /// clase para configurar los métodos de publicación de lectura
    /// </summary>
    /// <param name="hubContext"></param>
    /// <param name="logger"></param>
    public class SignalRTelemetryPublisher(IHubContext<TelemetryHub, ITelemetryClient> hubContext, ILogger<SignalRTelemetryPublisher> logger) : ITelemetryRealtimePublisher
    {
        /// <summary>
        /// publica la lectura de telemetria
        /// </summary>
        /// <param name="telemetry"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task PublishAsync(TelemetryRealtimeDto telemetry, CancellationToken ct)
        {
            try
            {
                await hubContext.Clients.Group(TelemetryHubGroups.ForMicrocontroller(telemetry.MicrocontrollerDeviceId)).TelemetryReceived(telemetry);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception,"No fue posible emitir telemetría de {DeviceId}.",telemetry.MicrocontrollerDeviceId);
            }
        }
    }
}
