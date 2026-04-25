using API.Application.DTOs.Requests.Users;
using API.Application.UseCases.Users;
using API.Web;
using API.Web.Attributes;
using API.Web.Mappers;
using API.Web.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Web.Controllers;

public class UsersController : BaseApiController
{
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly GetUserByIdUseCase _getUserByIdUseCase;
    private readonly GetAllUsersUseCase _getAllUsersUseCase;
    private readonly DeleteUserUseCase _deleteUserUseCase;

    public UsersController(
        CreateUserUseCase createUserUseCase,
        GetUserByIdUseCase getUserByIdUseCase,
        GetAllUsersUseCase getAllUsersUseCase,
        DeleteUserUseCase deleteUserUseCase)
    {
        _createUserUseCase = createUserUseCase;
        _getUserByIdUseCase = getUserByIdUseCase;
        _getAllUsersUseCase = getAllUsersUseCase;
        _deleteUserUseCase = deleteUserUseCase;
    }

    /// <summary>
    /// Cria um novo usuário.
    /// </summary>
    /// <remarks>
    /// Exemplo de body:
    ///
    ///     POST /api/users
    ///     {
    ///       "email": "sam@test.com",
    ///       "displayName": "Sam",
    ///       "password": "password123"
    ///     }
    /// </remarks>
    [SwaggerOperation(Summary = "Criar usuário", Description = "Cria um usuário e retorna os dados públicos do usuário criado.")]
    [Status201Created(typeof(UserResponse))]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserInput input)
    {
        var user = await _createUserUseCase.CreateAsync(input);
        var response = user.ToUserResponse();
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id },
            ApiResponse<UserResponse>.SuccessResponse(response));
    }

    /// <summary>
    /// Busca um usuário por ID.
    /// </summary>
    [SwaggerOperation(Summary = "Buscar usuário por ID", Description = "Retorna um único usuário quando encontrado.")]
    [ProducesOkApiResponse(typeof(UserResponse))]
    [ProducesNotFoundApiResponse]
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _getUserByIdUseCase.GetByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse.ErrorResponse("Usuário não encontrado", "USER_NOT_FOUND"));

        var response = user.ToUserResponse();
        return Ok(ApiResponse<UserResponse>.SuccessResponse(response));
    }

    /// <summary>
    /// Lista todos os usuários.
    /// </summary>
    [SwaggerOperation(Summary = "Listar usuários", Description = "Retorna uma coleção de usuários cadastrados.")]
    [ProducesOkApiResponse(typeof(IEnumerable<UserResponse>))]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _getAllUsersUseCase.GetAllAsync();
        var responses = users.ToUserResponseList();
        return Ok(ApiResponse<IEnumerable<UserResponse>>.SuccessResponse(responses));
    }

    /// <summary>
    /// Remove um usuário por ID.
    /// </summary>
    [SwaggerOperation(Summary = "Excluir usuário", Description = "Exclui o usuário informado. Retorna 204 quando a exclusão é realizada.")]
    [ProducesNoContentApiResponse]
    [ProducesNotFoundApiResponse]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var deleted = await _deleteUserUseCase.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse.ErrorResponse("Usuário não encontrado", "USER_NOT_FOUND"));

        return NoContent();
    }
}
