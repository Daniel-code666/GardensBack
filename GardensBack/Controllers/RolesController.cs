using GardensBack.Application.Roles.Abstractions;
using GardensBack.Application.Roles.Dtos;
using GardensBack.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GardensBack.Controllers
{
    /// <summary>
    /// Administra las consultas y operaciones disponibles para los roles del sistema.
    /// </summary>
    /// <param name="roleService">Servicio de aplicación para la gestión de roles.</param>
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController(IRoleService roleService) : ControllerBase
    {
        private readonly IRoleService _roleService = roleService;

        /// <summary>
        /// Crea un nuevo rol.
        /// </summary>
        /// <param name="role">Datos del rol que se desea crear.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El rol creado.</returns>
        [HttpPost("CreateRole")]
        [Authorize(Roles = BaseRoles.Admin)]
        public async Task<ActionResult<RoleRegisterRequest>> CreateRole([FromBody] RoleRegisterRequest role, CancellationToken ct)
            => Ok(await _roleService.CreateRole(role, ct));

        /// <summary>
        /// Busca un rol por su nombre.
        /// </summary>
        /// <param name="name">Nombre exacto del rol.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El rol encontrado.</returns>
        [HttpGet("FindByName")]
        public async Task<ActionResult<RoleRegisterRequest>> FindByName([FromQuery] string name, CancellationToken ct)
            => Ok(await _roleService.FindByName(name, ct));

        /// <summary>
        /// Obtiene los roles, opcionalmente filtrados por su estado activo.
        /// </summary>
        /// <param name="active">Estado activo por el cual filtrar; si se omite, devuelve todos.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>La colección de roles encontrados.</returns>
        [HttpGet("GetAllRoles")]
        public async Task<ActionResult<IEnumerable<RoleRegisterRequest>>> GetAllRoles([FromQuery] bool? active, CancellationToken ct)
            => Ok(await _roleService.GetAllRoles(active, ct));

        /// <summary>
        /// Actualiza los datos de un rol existente.
        /// </summary>
        /// <param name="role">Datos actualizados del rol, incluido su identificador.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns><see langword="true"/> cuando la actualización se guarda correctamente.</returns>
        [HttpPut("UpdateRole")]
        [Authorize(Roles = BaseRoles.Admin)]
        public async Task<ActionResult<bool>> UpdateRole([FromBody] RoleRegisterRequest role, CancellationToken ct)
            => Ok(await _roleService.UpdateRole(role, ct));

    }
}
