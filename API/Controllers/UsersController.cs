using Microsoft.AspNetCore.Mvc;
using API.Application.UseCases.Users;
using API.Application.Ports;

namespace API.Controllers;

/// <summary>
/// Controller de entrada (Adapter In) para gerenciar usuários.
/// Mantém-se fino, apenas orquestrando chamadas aos use cases.
/// A lógica de negócio está nos use cases.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Cria um novo usuário.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserInput input)
    {
        // Postman: POST http://localhost:5001/api/users
        // Body (raw JSON): { "email": "joao@example.com", "displayname": "Joao" }
        try
        {
            var useCase = new CreateUserUseCase(_userRepository);
            var user = await useCase.ExecuteAsync(input);

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
        // Postman: GET http://localhost:5001/api/users/{id}
        // Exemplo: GET http://localhost:5001/api/users/SEU_ID_AQUI
        var useCase = new GetUserByIdUseCase(_userRepository);
        var user = await useCase.ExecuteAsync(id);

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
        // Postman: GET http://localhost:5001/api/users
        var users = await _userRepository.GetAllAsync();
        return Ok(users);
    }

    /// <summary>
    /// Deleta um usuário por ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        // Postman: DELETE http://localhost:5001/api/users/{id}
        // Exemplo: DELETE http://localhost:5001/api/users/SEU_ID_AQUI
        var userExists = await _userRepository.GetByIdAsync(id);
        if (userExists == null)
            return NotFound(new { error = "Usuário não encontrado" });

        await _userRepository.RemoveAsync(id);
        await _userRepository.SaveChangesAsync();

        return NoContent();
    }
}
