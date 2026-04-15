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
        var users = await _userRepository.GetAllAsync();
        return Ok(users);
    }

    /// <summary>
    /// Deleta um usuário por ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var userExists = await _userRepository.GetByIdAsync(id);
        if (userExists == null)
            return NotFound(new { error = "Usuário não encontrado" });

        await _userRepository.RemoveAsync(id);
        await _userRepository.SaveChangesAsync();

        return NoContent();
    }
}
