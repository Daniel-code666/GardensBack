using GardensBack.Application.Common.Exceptions;
using AutoMapper;
using GardensBack.Application.DeviceConnectivity.Abstractions;
using GardensBack.Application.DeviceConnectivity.Dtos;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.DeviceConnectivity.Services
{
    public class DeviceConnectivityConfigurationService(IDeviceConnectivityConfigurationRepository repository, IMapper mapper) : IDeviceConnectivityConfigurationService
    {
        private readonly IDeviceConnectivityConfigurationRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<DeviceConnectivityConfigurationResponse> Get(CancellationToken ct)
        {
            DeviceConnectivityConfiguration configuration = await _repository.GetDefault(ct)
                ?? throw new NotFoundException("La configuración de conectividad para dispositivos no ha sido creada.");

            return MapResponse(configuration);
        }

        public async Task<DeviceConnectivityConfigurationResponse> Save(SaveDeviceConnectivityConfigurationRequest request, CancellationToken ct)
        {
            request.Validate();

            DeviceConnectivityConfiguration? configuration = await _repository.GetDefault(ct);
            if (configuration is null)
            {
                configuration = _mapper.Map<DeviceConnectivityConfiguration>(request);
                await _repository.Add(configuration, ct);
            }
            else
            {
                _mapper.Map(request, configuration);
            }

            configuration.Active = true;

            await _repository.SaveChanges(ct);
            return MapResponse(configuration);
        }

        private DeviceConnectivityConfigurationResponse MapResponse(DeviceConnectivityConfiguration configuration)
        {
            string apiScheme = configuration.ApiUseTls ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            string mqttScheme = configuration.MqttUseTls ? "mqtts" : "mqtt";
            DeviceConnectivityConfigurationResponse response = _mapper.Map<DeviceConnectivityConfigurationResponse>(configuration);
            response.ApiBaseUrl = BuildEndpoint(apiScheme, configuration.ApiHost, configuration.ApiPort);
            response.MqttEndpoint = BuildEndpoint(mqttScheme, configuration.MqttHost, configuration.MqttPort);

            return response;
        }

        private static string BuildEndpoint(string scheme, string host, int port)
            => new UriBuilder(scheme, host, port).Uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }
}
