using GardensBack.Application.Abstractions;
using GardensBack.Application.DeviceProvisioning.Abstractions;
using GardensBack.Application.DeviceConnectivity.Abstractions;
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

            return services;
        }

        public static void ConfigureDatabase(DbContextOptionsBuilder optionsBuilder, string ConnectionString)
            => optionsBuilder.UseNpgsql(ConnectionString);
    }
}
