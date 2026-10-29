using GardensBack.Application.Common.Results;
using GardensBack.Application.Users.Abstractions;
using GardensBack.Application.Users.Dtos;
using GardensBack.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GardensBack.Controllers
{
    /// <summary>
    /// Expone las operaciones de registro, consulta, actualización y desactivación de usuarios.
    /// </summary>
    /// <param name="userService">Servicio de aplicación para la gestión de usuarios.</param>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        /// <summary>
        /// Crea un usuario desde una operación administrativa.
        /// </summary>
        /// <param name="user">Datos del usuario que se desea crear.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El resultado de la creación del usuario.</returns>
        [HttpPost("CreateUser")]
        [Authorize(Roles = BaseRoles.Admin)]
        //[AllowAnonymous]
        public async Task<ActionResult<OperationResult<UserRegisterRequest>>> CreateUser([FromBody] UserRegisterRequest user, CancellationToken ct)
            => Ok(await _userService.CreateUser(user, ct));

        /// <summary>
        /// Registra anónimamente un usuario con el rol base configurado.
        /// </summary>
        /// <param name="user">Datos del usuario que se desea registrar.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El resultado del registro.</returns>
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<OperationResult<UserRegisterRequest>>> Register([FromBody] UserRegisterRequest user, CancellationToken ct)
            => Ok(await _userService.Register(user, ct));

        /// <summary>
        /// Crea un usuario utilizando el ejecutor centralizado de operaciones de persistencia.
        /// </summary>
        /// <param name="user">Datos del usuario que se desea crear.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El resultado de la creación del usuario.</returns>
        [HttpPost("CreateUserMiddleware")]
        [Authorize(Roles = BaseRoles.Admin)]
        public async Task<ActionResult<OperationResult<UserRegisterRequest>>> CreateUserMiddleware([FromBody] UserRegisterRequest user, CancellationToken ct)
            => Ok(await _userService.CreateUserMiddleware(user, ct));

        /// <summary>
        /// Actualiza los datos de un usuario existente.
        /// </summary>
        /// <param name="user">Datos actualizados del usuario, incluido su identificador.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El resultado de la actualización.</returns>
        [HttpPut("UpdateUser")]
        public async Task<ActionResult<OperationResult<UserRegisterRequest>>> UpdateUser([FromBody] UserRegisterRequest user, CancellationToken ct)
            => Ok(await _userService.UpdateUser(user, ct));

        /// <summary>
        /// Desactiva un usuario por su identificador.
        /// </summary>
        /// <param name="id">Identificador del usuario.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El resultado de la desactivación.</returns>
        [HttpDelete("DeleteUser/{id:int}")]
        public async Task<ActionResult<OperationResult<int>>> DeleteUser([FromRoute] int id, CancellationToken ct)
            => Ok(await _userService.DeleteUser(id, ct));

        /// <summary>
        /// Obtiene un usuario activo por su identificador.
        /// </summary>
        /// <param name="id">Identificador del usuario.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El usuario encontrado.</returns>
        [HttpGet("GetUserById/{id:int}")]
        public async Task<ActionResult<OperationResult<UserRegisterRequest>>> GetUserById([FromRoute] int id, CancellationToken ct)
            => Ok(await _userService.GetUserById(id, ct));

        /// <summary>
        /// Obtiene un usuario por su correo electrónico.
        /// </summary>
        /// <param name="email">Correo electrónico del usuario.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>El usuario encontrado.</returns>
        [HttpGet("GetUserByEmail")]
        public async Task<ActionResult<OperationResult<UserRegisterRequest>>> GetUserByEmail([FromQuery] string email, CancellationToken ct)
            => Ok(await _userService.GetUserByEmail(email, ct));

        /// <summary>
        /// Obtiene los usuarios filtrados opcionalmente por rol.
        /// </summary>
        /// <param name="roleId">Identificador opcional del rol.</param>
        /// <param name="roleName">Nombre opcional del rol.</param>
        /// <param name="ct">Token de cancelación de la solicitud.</param>
        /// <returns>La colección de usuarios encontrados.</returns>
        [HttpGet("GetAllUsers")]
        [Authorize(Roles = BaseRoles.Admin)]
        public async Task<ActionResult<OperationResult<IEnumerable<UserRegisterRequest>>>> GetAllUsers([FromQuery] int? roleId,[FromQuery] string? roleName,CancellationToken ct)
            => Ok(await _userService.GetAllUsers(roleId, roleName, ct));

    }
}
