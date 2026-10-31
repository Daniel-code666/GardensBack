using GardensBack.Application.Authentication.Abstractions;
using GardensBack.Application.Authentication.Repositories;
using GardensBack.Application.Common.Mapping;
using GardensBack.Application.Common.Security;
using GardensBack.Application.DeviceProvisioning.Abstractions;
using GardensBack.Application.DeviceConnectivity.Abstractions;
using GardensBack.Application.DeviceConnectivity.Services;
using GardensBack.Application.Roles.Abstractions;
using GardensBack.Application.Roles.Repositories;
using GardensBack.Application.Users.Abstractions;
using GardensBack.Application.Users.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GardensBack.Application
{
    using GardensBack.Application.DeviceInventory.Abstractions;
    using GardensBack.Application.DeviceInventory.Repositories;
    using GardensBack.Application.DeviceProvisioning.Services;
    using GardensBack.Application.FirmwareReleases.Abstractions;
    using GardensBack.Application.FirmwareReleases.Repositories;

    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(config => 
            {
                config.AddProfile<ApplicationAppProfile>();
            });

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<JwtTokenGenerator>();
            services.AddScoped<IDeviceProvisioningService, DeviceProvisioningService>();
            services.AddScoped<IDeviceConnectivityConfigurationService, DeviceConnectivityConfigurationService>();
            services.AddScoped<IDeviceInventoryService, DeviceInventoryService>();
            services.AddScoped<IFirmwareReleaseService, FirmwareReleaseService>();
            services.AddScoped<IFirmwareManifestService, FirmwareManifestService>();

            return services;
        }
    }
}
