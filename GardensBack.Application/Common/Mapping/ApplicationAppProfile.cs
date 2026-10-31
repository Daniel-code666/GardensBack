using AutoMapper;
using GardensBack.Application.DeviceConnectivity.Dtos;
using GardensBack.Application.DeviceProvisioning.Constants;
using GardensBack.Application.DeviceProvisioning.Dtos.Device;
using GardensBack.Application.DeviceProvisioning.Dtos.Web;
using GardensBack.Application.DeviceProvisioning.Enums;
using GardensBack.Application.FirmwareReleases.Constants;
using GardensBack.Application.FirmwareReleases.Dtos;
using GardensBack.Application.Roles.Dtos;
using GardensBack.Application.Users.Dtos;
using GardensBack.Domain.Entities;
using GardensBack.Domain.Enums;

namespace GardensBack.Application.Common.Mapping
{
    public class ApplicationAppProfile : Profile
    {
        public ApplicationAppProfile()
        {
            CreateMap<UserRegisterRequest, User>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.PasswordHash, o => o.MapFrom(x => x.Password))
                .ReverseMap()
                .ForMember(x => x.Id, o => o.MapFrom(x => x.Id))
                .ForMember(x => x.Password, o => o.Ignore())
                .ForMember(x => x.RoleName, o => o.MapFrom(x => x.Role.Name));

            CreateMap<RoleRegisterRequest, Role>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ReverseMap()
                .ForMember(x => x.Id, o => o.MapFrom(x => x.Id));

            CreateMap<UserLoginResponse, User>()
                .ForMember(x => x.PasswordHash, o => o.Ignore())
                .ForMember(x => x.RoleId, o => o.MapFrom(x => x.RoleId))
                .ReverseMap()
                .ForMember(x => x.RoleName, o => o.MapFrom(x => x.Role.Name))
                .ForMember(x => x.RoleId, o => o.MapFrom(x => x.RoleId));

            CreateMap<SaveDeviceConnectivityConfigurationRequest, DeviceConnectivityConfiguration>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.ConfigurationKey, o => o.Ignore())
                .ForMember(x => x.Active, o => o.Ignore())
                .ForMember(x => x.CreationDate, o => o.Ignore())
                .ForMember(x => x.UpdateDate, o => o.Ignore());

            CreateMap<DeviceConnectivityConfiguration, DeviceConnectivityConfigurationResponse>()
                .ForMember(x => x.ApiBaseUrl, o => o.Ignore())
                .ForMember(x => x.MqttEndpoint, o => o.Ignore());

            CreateMap<StartDeviceProvisioningRequest, MicrocontrollerDevice>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.PublicId, o => o.Ignore())
                .ForMember(x => x.ProvisioningState, o => o.Ignore())
                .ForMember(x => x.ClaimedAt, o => o.Ignore())
                .ForMember(x => x.UnclaimedAt, o => o.Ignore())
                .ForMember(x => x.LastRecoveryCommandVersion, o => o.Ignore())
                .ForMember(x => x.DeviceIp, o => o.Ignore())
                .ForMember(x => x.DeviceSSID, o => o.Ignore())
                .ForMember(x => x.UserId, o => o.Ignore())
                .ForMember(x => x.User, o => o.Ignore())
                .ForMember(x => x.DeviceClaim, o => o.Ignore())
                .ForMember(x => x.MqttCredential, o => o.Ignore())
                .ForMember(x => x.Devices, o => o.Ignore())
                .ForMember(x => x.DeviceCommands, o => o.Ignore())
                .ForMember(x => x.DeviceFirmwareUpdates, o => o.Ignore())
                .ForMember(x => x.Active, o => o.Ignore())
                .ForMember(x => x.CreationDate, o => o.Ignore())
                .ForMember(x => x.UpdateDate, o => o.Ignore());

            CreateMap<ClaimRegistrationRequest, DeviceClaim>(MemberList.None)
                .ForMember(destination => destination.RequestId, options =>
                    options.MapFrom(source => source.RequestId))
                .ForMember(destination => destination.ContractVersion, options =>
                    options.MapFrom(source => source.ContractVersion));

            CreateMap<DeviceClaim, ClaimResponse>()
                .ForMember(destination => destination.Type, options =>
                    options.MapFrom(source => DeviceProvisioningContract.ClaimResponseType))
                .ForMember(destination => destination.ContractVersion, options =>
                    options.MapFrom(source => source.ContractVersion))
                .ForMember(destination => destination.DeviceId, options =>
                    options.MapFrom(source => source.MicroCtrlDevice.DeviceId!))
                .ForMember(destination => destination.RequestId, options =>
                    options.MapFrom(source => source.RequestId!))
                .ForMember(destination => destination.Decision, options =>
                    options.MapFrom(source =>
                        source.DeviceClaimStatus == DeviceClaimStatus.Approved
                            ? ClaimDecision.Approved
                            : source.DeviceClaimStatus == DeviceClaimStatus.Expired
                                ? ClaimDecision.Expired
                                : ClaimDecision.Rejected))
                .ForMember(destination => destination.UserId, options =>
                    options.MapFrom(source =>
                        source.DeviceClaimStatus == DeviceClaimStatus.Approved
                            ? source.MicroCtrlDevice.UserId.ToString()
                            : string.Empty))
                .ForMember(destination => destination.GardenId, options =>
                    options.MapFrom(source =>
                        source.DeviceClaimStatus == DeviceClaimStatus.Approved
                            ? source.MicroCtrlDevice.PublicId.ToString("N")
                            : string.Empty))
                .ForMember(destination => destination.ErrorCode, options =>
                    options.MapFrom(source =>
                        source.DeviceClaimStatus == DeviceClaimStatus.Approved
                            ? string.Empty
                            : source.ErrorCode ?? "claim_rejected"))
                .ForMember(destination => destination.AuthenticationTag, options =>
                    options.MapFrom(source => source.ResponseAuthenticationTag ?? string.Empty));

            CreateMap<MqttCredential, MqttCredentialsEnvelopeResponse>()
                .ForMember(destination => destination.Type, options =>
                    options.MapFrom(source => DeviceProvisioningContract.MqttCredentialsEnvelopeType))
                .ForMember(destination => destination.ContractVersion, options =>
                    options.MapFrom(source => DeviceProvisioningContract.Version))
                .ForMember(destination => destination.DeviceId, options =>
                    options.MapFrom(source => source.MicroCtrlDevice.DeviceId!))
                .ForMember(destination => destination.RequestId, options =>
                    options.MapFrom(source => source.RequestId!))
                .ForMember(destination => destination.Nonce, options =>
                    options.MapFrom(source => source.ResponseNonce!))
                .ForMember(destination => destination.Ciphertext, options =>
                    options.MapFrom(source => source.ResponseCiphertext!))
                .ForMember(destination => destination.AuthenticationTag, options =>
                    options.MapFrom(source => source.ResponseAuthenticationTag!));

            CreateMap<CreateFirmwareReleaseRequest, FirmwareRelease>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.Status, o => o.Ignore())
                .ForMember(x => x.PublishedAt, o => o.Ignore())
                .ForMember(x => x.DeviceFirmwareUpdates, o => o.Ignore())
                .ForMember(x => x.Active, o => o.Ignore())
                .ForMember(x => x.CreationDate, o => o.Ignore())
                .ForMember(x => x.UpdateDate, o => o.Ignore());

            CreateMap<FirmwareRelease, FirmwareReleaseResponse>();

            CreateMap<UpdateFirmwareReleaseRequest, FirmwareRelease>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.ReleaseId, o => o.Ignore())
                .ForMember(x => x.Status, o => o.Ignore())
                .ForMember(x => x.PublishedAt, o => o.Ignore())
                .ForMember(x => x.DeviceFirmwareUpdates, o => o.Ignore())
                .ForMember(x => x.Active, o => o.Ignore())
                .ForMember(x => x.CreationDate, o => o.Ignore())
                .ForMember(x => x.UpdateDate, o => o.Ignore());

            CreateMap<FirmwareRelease, FirmwareManifestResponse>()
                .ForMember(x => x.Type, o => o.MapFrom(_ => FirmwareReleaseContract.ManifestType))
                .ForMember(x => x.Size, o => o.MapFrom(x => x.SizeBytes));
        }
    }
}
