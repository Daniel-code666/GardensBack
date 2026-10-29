using GardensBack.Application.Common.Results;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.Abstractions
{
    public interface IRoleRepository
    {
        Task<Role> FindByName(string name, CancellationToken ct);
        Task<IEnumerable<Role>> GetAllRoles(bool? active, CancellationToken ct);
        Task<Role> FindById(int id, CancellationToken ct);
        Task<bool> HasUser(int id, CancellationToken ct);
        Task<Role> CreateRole(Role role, CancellationToken ct);
        Task<bool> SaveAsync(CancellationToken ct);
    }
}
