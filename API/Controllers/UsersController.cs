using Microsoft.AspNetCore.Mvc;
using API.Application.Services;
using API.Application.UseCases.Users;
using API.Infrastructure.Http;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : BaseApiController
{
    private readonly UserApplicationService _userApplicationService;

    public UsersController(UserApplicationService userApplicationService)
    {
        _userApplicationService = userApplicationService;
    }

    // POST http://localhost:5001/api/users
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserInput input)
    {
        var user = await _userApplicationService.CreateUserAsync(input);
        var response = user.ToUserResponse();
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id },
            ApiResponse<object>.SuccessResponse(response));
    }

    // GET http://localhost:5001/api/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userApplicationService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Usuário não encontrado", "USER_NOT_FOUND"));

        var response = user.ToUserResponse();
        return Ok(ApiResponse<object>.SuccessResponse(response));
    }

    // GET http://localhost:5001/api/users
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userApplicationService.GetAllUsersAsync();
        var responses = users.ToUserResponseList();
        return Ok(ApiResponse<object>.SuccessResponse(responses));
    }

    // DELETE http://localhost:5001/api/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var deleted = await _userApplicationService.DeleteUserAsync(id);
        if (!deleted)
            return NotFound(ApiResponse.ErrorResponse("Usuário não encontrado", "USER_NOT_FOUND"));

        return NoContent();
    }
}
