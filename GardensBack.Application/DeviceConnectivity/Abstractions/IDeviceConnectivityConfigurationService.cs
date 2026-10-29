using GardensBack.Application.DeviceConnectivity.Dtos;

namespace GardensBack.Application.DeviceConnectivity.Abstractions
{
    public interface IDeviceConnectivityConfigurationService
    {
        Task<DeviceConnectivityConfigurationResponse> Get(CancellationToken ct);
        Task<DeviceConnectivityConfigurationResponse> Save(SaveDeviceConnectivityConfigurationRequest request, CancellationToken ct);
    }
}
