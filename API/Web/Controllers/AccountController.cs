using API.Application.DTOs.Requests.Account;
using API.Application.UseCases.Account;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Controllers;


public class AccountController : BaseApiController
{
    private readonly CreateAccountUseCase _createAccountUseCase;

    public AccountController(CreateAccountUseCase createAccountUseCase)
    {
        _createAccountUseCase = createAccountUseCase;
    }

    // POST http://localhost:5001/api/account/register  (Body JSON)
    [Consumes("application/json")]
    [HttpPost("register")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountInput input)
    {
        var user = await _createAccountUseCase.ExecuteAsync(input);
        return Ok(user);
    }

    // POST http://localhost:5001/api/account/register?email=sam@test.com&password=password&displayName=Sam
    [HttpPost("register")]
    public async Task<IActionResult> CreateAccountByQuery([FromQuery] CreateAccountInput input)
    {
        var user = await _createAccountUseCase.ExecuteAsync(input);
        return Ok(user);
    }
}
