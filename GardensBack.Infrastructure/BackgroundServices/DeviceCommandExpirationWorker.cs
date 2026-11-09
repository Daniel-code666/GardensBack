using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GardensBack.Infrastructure.BackgroundServices
{
    public sealed class DeviceCommandExpirationWorker(IServiceScopeFactory scopeFactory, TimeProvider timeProvider, 
        IOptions<DeviceCommandExpirationOptions> options, ILogger<DeviceCommandExpirationWorker> logger) : BackgroundService
    {
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(options.Value.SweepIntervalSeconds);

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            await ExpireCommands(ct);

            using var timer = new PeriodicTimer(_interval, timeProvider);

            try
            {
                while (await timer.WaitForNextTickAsync(ct))
                {
                    await ExpireCommands(ct);
                }
            }
            catch (OperationCanceledException)
                when (ct.IsCancellationRequested)
            {
                // Cierre normal de la aplicación.
            }
        }

        private async Task ExpireCommands(CancellationToken ct)
        {
            try
            {
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

                IDeviceInventoryService service = scope.ServiceProvider.GetRequiredService<IDeviceInventoryService>();

                DateTime now = timeProvider.GetUtcNow().UtcDateTime;

                int expiredCommands = await service.ExpireDeviceCommands(now, ct);

                if (expiredCommands > 0)
                {
                    logger.LogInformation("Se marcaron {count} comandos como expirados", expiredCommands);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ocurrió un error expirando comandos de dispositivos.");
            }
        }
    }
}
