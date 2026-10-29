using GardensBack.Application.Roles.Dtos;

namespace GardensBack.Application.Roles.Abstractions
{
    public interface IRoleService
    {
        Task<RoleRegisterRequest> FindByName(string name, CancellationToken ct);
        Task<IEnumerable<RoleRegisterRequest>> GetAllRoles(bool? active, CancellationToken ct);
        Task<RoleRegisterRequest> CreateRole(RoleRegisterRequest role, CancellationToken ct);
        Task<bool> UpdateRole(RoleRegisterRequest role, CancellationToken ct);
    }
}
