using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Security;
using GardensBack.Application.DeviceConnectivity.Abstractions;
using GardensBack.Application.DeviceInventory.Abstractions;
using GardensBack.Application.DeviceInventory.Dtos;
using GardensBack.Application.DeviceProvisioning.Abstractions;
using GardensBack.Application.FirmwareUpdates.Abstractions;
using GardensBack.Application.FirmwareUpdates.Dtos;
using GardensBack.Infrastructure.Firmware;
using GardensBack.Infrastructure.Messaging;
using GardensBack.Infrastructure.Persistance;
using GardensBack.Infrastructure.Persistance.Repositories;
using GardensBack.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GardensBack.Domain
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options => ConfigureDatabase(options, connectionString));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IDeviceProvisioningRepository, DeviceProvisioningRepository>();
            services.AddScoped<IDeviceConnectivityConfigurationRepository, DeviceConnectivityConfigurationRepository>();
            services.AddSingleton<IProvisioningTokenService, ProvisioningTokenService>();
            services.AddSingleton<IClaimResponseSigner, ClaimResponseSigner>();
            services.AddSingleton<IMqttCredentialEnvelopeService, MqttCredentialEnvelopeService>();
            services.AddScoped<IDeviceInventoryRepository, DeviceInventoryRepository>();
            services.AddScoped<IFirmwareReleaseRepository, FirmwareReleaseRepository>();
            services.AddScoped<IFirmwareReleasePublicationValidator, FirmwareReleasePublicationValidator>();
            services.AddScoped<IDeviceFirmwareUpdateRepository, DeviceFirmwareUpdateRepository>();
            services.AddSingleton<IDeviceCommandKeyProtector, DeviceCommandKeyProtector>();
            services.AddSingleton<ICommandAuthenticationService<OtaUpdateCommand>, OtaCommandAuthenticationService>();
            services.AddSingleton<ICommandAuthenticationService<DeviceActionMessage>, DeviceActionAuthenticationService>();
            services.AddScoped<IDeviceControlPublisher, MqttDeviceControlPublisher>();
            services.AddScoped<IDeviceActionPublisher, MqttDeviceActionPublisher>();
            services.AddHostedService<MqttFirmwareUpdateStatusConsumer>();
            services.AddHostedService<MqttInventoryConsumer>();
            services.AddHostedService<MqttTelemetryConsumer>();
            services.AddHostedService<MqttDeviceActionResponseConsumer>();

            return services;
        }

        public static void ConfigureDatabase(DbContextOptionsBuilder optionsBuilder, string ConnectionString)
            => optionsBuilder.UseNpgsql(ConnectionString);
    }
}
