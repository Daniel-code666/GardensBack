using AutoMapper;
using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.Common.Results;
using GardensBack.Application.Common.Security;
using GardensBack.Application.Users.Abstractions;
using GardensBack.Application.Users.Dtos;
using GardensBack.Domain.Constants;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.Users.Repositories
{
    public class UserService(IUserRepository userRepository, IRoleRepository roleRepository, IMapper mapper) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IRoleRepository _roleRepository = roleRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<OperationResult<UserRegisterRequest>> CreateUser(UserRegisterRequest user, CancellationToken ct)
        {
            ValidateRequest(user);

            Role role = await GetRoleById(user.RoleId, ct);
            User userToCreate = _mapper.Map<UserRegisterRequest, User>(user);
            userToCreate.Role = role;
            userToCreate.PasswordHash = PasswordUtility.Hash(userToCreate, user.Password);

            var result = await _userRepository.CreateUser(userToCreate, ct);

            if (result.Result != null && result.Successful)
            {
                return OperationResult<UserRegisterRequest>.Success(nameof(CreateUser), _mapper.Map<User, UserRegisterRequest>(result.Result));
            }

            return OperationResult<UserRegisterRequest>.Failure(nameof(CreateUser), 400, "Usuario no creado");
        }

        public async Task<OperationResult<UserRegisterRequest>> Register(UserRegisterRequest user, CancellationToken ct)
        {
            ValidateRequest(user);

            Role role = await GetRoleByName(BaseRoles.NormalUser, ct);
            User userToCreate = _mapper.Map<UserRegisterRequest, User>(user);
            userToCreate.Role = role;
            userToCreate.PasswordHash = PasswordUtility.Hash(userToCreate, user.Password);

            var result = await _userRepository.CreateUser(userToCreate, ct);

            if (result.Result != null && result.Successful)
            {
                return OperationResult<UserRegisterRequest>.Success(nameof(CreateUser), _mapper.Map<User, UserRegisterRequest>(result.Result));
            }

            return OperationResult<UserRegisterRequest>.Failure(nameof(CreateUser), 400, "Usuario no creado");
        }

        public async Task<OperationResult<UserRegisterRequest>> CreateUserMiddleware(UserRegisterRequest user, CancellationToken ct)
        {
            ValidateRequest(user);

            Role role = await GetRoleById(user.RoleId, ct);
            User userToCreate = _mapper.Map<UserRegisterRequest, User>(user);
            userToCreate.Role = role;
            userToCreate.PasswordHash = PasswordUtility.Hash(userToCreate, user.Password);

            var result = await _userRepository.CreateUserMiddleware(userToCreate, ct);

            if (result.Result != null && result.Successful)
            {
                return OperationResult<UserRegisterRequest>.Success(nameof(CreateUser), _mapper.Map<User, UserRegisterRequest>(result.Result));
            }

            return OperationResult<UserRegisterRequest>.Failure(nameof(CreateUser), 400, "Usuario no creado");
        }

        public async Task<OperationResult<UserRegisterRequest>> UpdateUser(UserRegisterRequest user, CancellationToken ct)
        {
            ValidateRequest(user);

            if (!user.Id.HasValue || user.Id.Value <= 0)
            {
                throw new ValidationException("User id required");
            }

            int userId = user.Id.Value;
            var foundUser = await _userRepository.GetUserById(userId, ct);

            if (!foundUser.Successful || foundUser.Result is null)
            {
                throw new NotFoundException("User not found");
            }

            var foundEmail = await _userRepository.GetUserByEmail(user.Email, ct);

            if (foundEmail.Successful && foundEmail.Result is not null && foundEmail.Result.Id != userId)
            {
                throw new ValidationException("Email already exists");
            }

            User userToUpdate = foundUser.Result;
            _mapper.Map(user, userToUpdate);
            userToUpdate.PasswordHash = PasswordUtility.Hash(userToUpdate, user.Password);

            if (await _userRepository.SaveChanges(ct))
            {
                return OperationResult<UserRegisterRequest>.Success(
                    nameof(UpdateUser),
                    _mapper.Map<User, UserRegisterRequest>(userToUpdate));
            }

            return OperationResult<UserRegisterRequest>.Failure(
                nameof(UpdateUser),
                400,
                "Usuario no actualizado");
        }

        public async Task<OperationResult<int>> DeleteUser(int id, CancellationToken ct)
        {
            if (await _userRepository.GetUserById(id, ct) is OperationResult<User> result && result.Result is not null && result.Successful)
            {
                return await _userRepository.DeleteUser(id, ct);
            }

            throw new NotFoundException("User not found");
        }

        public async Task<OperationResult<UserRegisterRequest>> GetUserById(int id, CancellationToken ct)
        {
            var result = await _userRepository.GetUserById(id, ct);

            if (result.Successful && result.Result is not null)
            {
                return OperationResult<UserRegisterRequest>.Success(nameof(GetUserById), _mapper.Map<User, UserRegisterRequest>(result.Result));
            }

            throw new NotFoundException("User not found");
        }

        public async Task<OperationResult<UserRegisterRequest>> GetUserByEmail(string email, CancellationToken ct)
        {
            var result = await _userRepository.GetUserByEmail(email, ct);

            if (result.Successful && result.Result is not null)
            {
                return OperationResult<UserRegisterRequest>.Success(nameof(GetUserById), _mapper.Map<User, UserRegisterRequest>(result.Result));
            }

            throw new NotFoundException("User not found");
        }

        public async Task<OperationResult<IEnumerable<UserRegisterRequest>>> GetAllUsers(int? roleId, string? roleName, CancellationToken ct)
        {
            var result = await _userRepository.GetAllUsers(roleId, roleName, ct);

            if (result.Successful && result.Result is not null)
            {
                return OperationResult<IEnumerable<UserRegisterRequest>>.Success(nameof(GetUserById), _mapper.Map<IEnumerable<User>, IEnumerable<UserRegisterRequest>>(result.Result));
            }

            return OperationResult<IEnumerable<UserRegisterRequest>>.Success(nameof(GetAllUsers), []);
        }

        private async Task<Role> GetRoleByName(string name, CancellationToken ct)
        {
            Role? foundRole = await _roleRepository.FindByName(name, ct);

            if (foundRole is not null)
            {
                return foundRole;
            }

            throw new NotFoundException("Role not found");
        }

        private async Task<Role> GetRoleById(int id, CancellationToken ct)
        {
            Role? foundRole = await _roleRepository.FindById(id, ct);

            if (foundRole is not null)
            {
                return foundRole;
            }

            throw new NotFoundException("Role not found");
        }

        private static void ValidateRequest(UserRegisterRequest request)
        {
            if (request is null) throw new ValidationException("Body required");

            request.Validate();
        }
    }
}
