using GardensBack.Application.Abstractions;
using GardensBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GardensBack.Infrastructure.Persistance.Repositories
{
    public class RoleRepository(ApplicationDbContext db) : IRoleRepository
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<Role> CreateRole(Role role, CancellationToken ct)
        {
            await _db.Roles.AddAsync(role, ct);
            await SaveAsync(ct);

            return role;
        }

        public async Task<Role> FindById(int id, CancellationToken ct)
            => (await _db.Roles.SingleOrDefaultAsync(x => x.Id == id, ct))!;

        public async Task<Role> FindByName(string name, CancellationToken ct)
            => (await _db.Roles.SingleOrDefaultAsync(x => x.Name == name, ct))!;

        public async Task<IEnumerable<Role>> GetAllRoles(bool? active, CancellationToken ct)
            => await _db.Roles.Where(x => !active.HasValue || x.Active == active).ToListAsync(ct);

        public async Task<bool> HasUser(int id, CancellationToken ct)
            => await _db.Users.AnyAsync(x => x.RoleId == id, ct);

        public async Task<bool> SaveAsync(CancellationToken ct)
            => await _db.SaveChangesAsync(ct) > 0;
    }
}
