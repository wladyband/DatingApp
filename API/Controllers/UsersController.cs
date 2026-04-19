using Microsoft.AspNetCore.Mvc;
using API.Application.Services;
using API.Application.UseCases.Users;
using API.Infrastructure.Http;

namespace API.Controllers;

/// <summary>
/// Controller de entrada (Adapter In) para gerenciar usuários.
/// Mantém-se fino, apenas orquestrando chamadas aos application services.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UsersController : BaseApiController
{
    private readonly UserApplicationService _userApplicationService;

    public UsersController(UserApplicationService userApplicationService)
    {
        _userApplicationService = userApplicationService;
    }

    /// <summary>
    /// Cria um novo usuário.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserInput input)
    {
        var user = await _userApplicationService.CreateUserAsync(input);
        var response = user.ToUserResponse();
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id },
            ApiResponse<object>.SuccessResponse(response));
    }

    /// <summary>
    /// Obtém um usuário por ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userApplicationService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado", "USER_NOT_FOUND"));

        var response = user.ToUserResponse();
        return Ok(ApiResponse<object>.SuccessResponse(response));
    }

    /// <summary>
    /// Obtém todos os usuários.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userApplicationService.GetAllUsersAsync();
        var responses = users.ToUserResponseList();
        return Ok(ApiResponse<object>.SuccessResponse(responses));
    }

    /// <summary>
    /// Deleta um usuário.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var deleted = await _userApplicationService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound(ApiResponse.ErrorResponse("Usuário não encontrado", "USER_NOT_FOUND"));

        return NoContent();
    }
}
