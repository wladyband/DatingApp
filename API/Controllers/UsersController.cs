using Microsoft.AspNetCore.Mvc;
using API.Application.UseCases.Users;

namespace API.Controllers;

/// <summary>
/// Controller de entrada (Adapter In) para gerenciar usuários.
/// Mantém-se fino, apenas orquestrando chamadas aos use cases.
/// A lógica de negócio está nos use cases.
/// </summary>

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
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserInput input)
    {
        // Postman: POST http://localhost:5000/api/users
        // Body (raw JSON): { "email": "joao@example.com", "displayname": "Joao" }
        try
        {
            var user = await _createUserUseCase.ExecuteAsync(input);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um usuário por ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        // Postman: GET http://localhost:5000/api/users/{id}
        // Exemplo: GET http://localhost:5000/api/users/SEU_ID_AQUI
        var user = await _getUserByIdUseCase.ExecuteAsync(id);

        if (user == null)
            return NotFound(new { error = "Usuário não encontrado" });

        return Ok(user);
    }

    /// <summary>
    /// Obtém todos os usuários.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        // Postman: GET http://localhost:5000/api/users
        var users = await _getAllUsersUseCase.ExecuteAsync();
        return Ok(users);
    }

    /// <summary>
    /// Deleta um usuário por ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        // Postman: DELETE http://localhost:5000/api/users/{id}
        // Exemplo: DELETE http://localhost:5000/api/users/SEU_ID_AQUI
        var deleted = await _deleteUserUseCase.ExecuteAsync(id);
        if (!deleted)
            return NotFound(new { error = "Usuário não encontrado" });

        return NoContent();
    }
}
