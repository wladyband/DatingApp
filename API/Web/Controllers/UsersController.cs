using API.Application.DTOs.Requests.Users;
using API.Application.UseCases.Users;
using API.Web;
using API.Web.Mappers;
using API.Web.Responses;
using Microsoft.AspNetCore.Mvc;

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

    // POST http://localhost:5001/api/users
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserInput input)
    {
        var user = await _createUserUseCase.ExecuteAsync(input);
        var response = user.ToUserResponse();
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id },
            ApiResponse<UserResponse>.SuccessResponse(response));
    }

    // GET http://localhost:5001/api/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _getUserByIdUseCase.ExecuteAsync(id);
        if (user == null)
            return NotFound(ApiResponse.ErrorResponse("Usuário não encontrado", "USER_NOT_FOUND"));

        var response = user.ToUserResponse();
        return Ok(ApiResponse<UserResponse>.SuccessResponse(response));
    }

    // GET http://localhost:5001/api/users
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _getAllUsersUseCase.ExecuteAsync();
        var responses = users.ToUserResponseList();
        return Ok(ApiResponse<IEnumerable<UserResponse>>.SuccessResponse(responses));
    }

    // DELETE http://localhost:5001/api/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var deleted = await _deleteUserUseCase.ExecuteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse.ErrorResponse("Usuário não encontrado", "USER_NOT_FOUND"));

        return NoContent();
    }
}
