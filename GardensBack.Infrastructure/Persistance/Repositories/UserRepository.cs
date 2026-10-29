using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Results;
using GardensBack.Domain.Constants;
using GardensBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GardensBack.Infrastructure.Persistance.Repositories
{
    public class UserRepository(ApplicationDbContext db, IUnitOfWork unitOfWork) : IUserRepository
    {
        private readonly ApplicationDbContext _db = db;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        // implementación normal
        public async Task<OperationResult<User>> CreateUser(User user, CancellationToken ct)
        {
            string operationName = nameof(CreateUser);
            await _db.Users.AddAsync(user, ct);
            return await _db.SaveChangesAsync(ct) > 0 ? OperationResult<User>.Success(operationName, user)
                : OperationResult<User>.Failure(operationName, 0, "No se ha creado el usuario");
        }

        // implementación con middleware
        public Task<OperationResult<User>> CreateUserMiddleware(User user, CancellationToken ct)
        {
            return _unitOfWork.ExecuteCommandAsync(
                nameof(CreateUser), 
                    async token => 
                    { 
                        await _db.Users.AddAsync(user, token);
                        return user; 
                    }, 
                    ct);
        }

        public async Task<OperationResult<int>> DeleteUser(int id, CancellationToken ct)
        {
            await _db.Users.Where(x => x.Id == id).ExecuteUpdateAsync(s => s.SetProperty(x => x.Active, false), ct);

            await SaveChanges(ct);

            return OperationResult<int>.Success(nameof(DeleteUser), id);
        }

        public async Task<OperationResult<IEnumerable<User>>> GetAllUsers(int? roleId, string? roleName, CancellationToken ct)
        {
            var result = await _db.Users.Include(x => x.Role)
                .Where(x => (!roleId.HasValue || x.RoleId == roleId) && (string.IsNullOrEmpty(roleName) || x.Role.Name == roleName)).ToListAsync(ct);

            return OperationResult<IEnumerable<User>>.Success(nameof(GetAllUsers), result);
        }

        public async Task<OperationResult<User>> GetUserByEmail(string email, CancellationToken ct)
            => OperationResult<User>.Success(nameof(GetUserByEmail), await _db.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.Email == email, ct));

        public async Task<OperationResult<User>> GetUserById(int id, CancellationToken ct)
            => OperationResult<User>.Success(nameof(GetUserById), await _db.Users.Include(x => x.Role).SingleOrDefaultAsync(x => x.Id == id && x.Active, ct));

        public async Task<OperationResult<string>> GetUserEmailById(int id, CancellationToken ct)
            => OperationResult<string>.Success(nameof(GetUserEmailById), await _db.Users.Include(x => x.Role)
                .Where(x => x.Id == id && x.Active).Select(x => x.Email).FirstOrDefaultAsync(ct));

        public async Task<bool> HasAdmin(CancellationToken ct)
            => await _db.Users.AnyAsync(x => x.Role.Name == BaseRoles.Admin && x.Active, ct);

        public async Task<bool> SaveChanges(CancellationToken ct)
        {
            return await _db.SaveChangesAsync(ct) > 0;
        }
    }
}
