using API.Application.UseCases.Account;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly CreateAccountUseCase _createAccountUseCase;

    public AccountController(CreateAccountUseCase createAccountUseCase)
    {
        _createAccountUseCase = createAccountUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountInput input)
    {
        try
        {
            var user = await _createAccountUseCase.ExecuteAsync(input);
            return Created($"/api/account/{user.Id}", user);
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
}
