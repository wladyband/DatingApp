using API.Application.Services;
using API.Application.UseCases.Account;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


public class AccountController : BaseApiController
{
    private readonly AccountApplicationService _accountApplicationService;

    public AccountController(AccountApplicationService accountApplicationService)
    {
        _accountApplicationService = accountApplicationService;
    }

    // POST http://localhost:5001/api/account/register  (Body JSON)
    [Consumes("application/json")]
    [HttpPost("register")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountInput input)
    {
        var user = await _accountApplicationService.CreateAccountAsync(input);
        return Ok(user);
    }

    // POST http://localhost:5001/api/account/register?email=sam@test.com&password=password&displayName=Sam
    [HttpPost("register")]
    public async Task<IActionResult> CreateAccountByQuery([FromQuery] CreateAccountInput input)
    {
        var user = await _accountApplicationService.CreateAccountAsync(input);
        return Ok(user);
    }
}
