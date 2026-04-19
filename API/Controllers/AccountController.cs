using API.Application.Services;
using API.Application.UseCases.Account;
using API.Infrastructure.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : BaseApiController
{
    private readonly AccountApplicationService _accountApplicationService;

    public AccountController(AccountApplicationService accountApplicationService)
    {
        _accountApplicationService = accountApplicationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountInput input)
    {
        var user = await _accountApplicationService.CreateAccountAsync(input);
        var response = user.ToAccountResponse();
        return Created($"/api/account/{user.Id}", ApiResponse<object>.SuccessResponse(response));
    }
}
