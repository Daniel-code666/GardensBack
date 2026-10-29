using AutoMapper;
using GardensBack.Application.Abstractions;
using GardensBack.Application.Common.Exceptions;
using GardensBack.Application.Roles.Abstractions;
using GardensBack.Application.Roles.Dtos;
using GardensBack.Domain.Entities;

namespace GardensBack.Application.Roles.Repositories
{
    public class RoleService(IRoleRepository roleRepository, IMapper mapper) : IRoleService
    {
        private readonly IRoleRepository _roleRepository = roleRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<RoleRegisterRequest> CreateRole(RoleRegisterRequest role, CancellationToken ct)
        {
            ValidateRequest(role);

            Role? foundRole = await _roleRepository.FindByName(role.Name, ct);

            if (foundRole is not null)
            {
                throw new ValidationException("Role name already exists");
            }

            Role roleToCreate = _mapper.Map<RoleRegisterRequest, Role>(role);
            Role createdRole = await _roleRepository.CreateRole(roleToCreate, ct);

            return _mapper.Map<Role, RoleRegisterRequest>(createdRole);
        }

        public async Task<RoleRegisterRequest> FindByName(string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ValidationException("Role name required");
            }

            Role? foundRole = await _roleRepository.FindByName(name, ct);

            return foundRole is null ? throw new NotFoundException("Role not found") : _mapper.Map<Role, RoleRegisterRequest>(foundRole);
        }

        public async Task<IEnumerable<RoleRegisterRequest>> GetAllRoles(bool? active, CancellationToken ct)
        {
            IEnumerable<Role> roles = await _roleRepository.GetAllRoles(active, ct);

            return _mapper.Map<IEnumerable<Role>, IEnumerable<RoleRegisterRequest>>(roles);
        }

        public async Task<bool> UpdateRole(RoleRegisterRequest role, CancellationToken ct)
        {
            ValidateRequest(role);

            if (!role.Id.HasValue || role.Id.Value <= 0)
            {
                throw new ValidationException("Role id required");
            }

            int roleId = role.Id.Value;
            Role? roleToUpdate = await _roleRepository.FindById(roleId, ct) ?? throw new NotFoundException("Role not found");
            Role? foundRole = await _roleRepository.FindByName(role.Name, ct);

            if (foundRole is not null && foundRole.Id != roleId)
            {
                throw new ValidationException("Role name already exists");
            }

            _mapper.Map(role, roleToUpdate);

            return await _roleRepository.SaveAsync(ct);
        }

        private static void ValidateRequest(RoleRegisterRequest request)
        {
            if (request is null)
            {
                throw new ValidationException("Body required");
            }

            request.Validate();
        }
    }
}
