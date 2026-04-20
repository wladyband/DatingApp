using API.Application.DTOs.Requests.Account;
using API.Application.DTOs.Requests.Login;
using API.Application.UseCases.Account;
using API.Application.UseCases.Login;
using API.Web;
using API.Web.Attributes;
using API.Web.Mappers;
using API.Web.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Web.Controllers;


public class AccountController : BaseApiController
{
    private readonly CreateAccountUseCase _createAccountUseCase;

    private readonly LoginUseCase _loginUseCase;

    public AccountController(CreateAccountUseCase createAccountUseCase, LoginUseCase loginUseCase)
    {
        _createAccountUseCase = createAccountUseCase;
        _loginUseCase = loginUseCase;
    }

    /// <summary>
    /// Registra uma nova conta com payload JSON.
    /// </summary>
    /// <remarks>
    /// Exemplo de body:
    ///
    ///     POST /api/account/register
    ///     {
    ///       "email": "sam@test.com",
    ///       "displayname": "Sam",
    ///       "password": "password123"
    ///     }
    /// </remarks>
    [SwaggerOperation(Summary = "Registrar conta", Description = "Cria uma nova conta e retorna os dados públicos da conta criada.")]
    [Consumes("application/json")]
    [ProducesCreatedApiResponse(typeof(AccountResponse))]
    [HttpPost("register")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountInput input)
    {
        var user = await _createAccountUseCase.ExecuteAsync(input);
        var response = user.ToAccountResponse();
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<AccountResponse>.SuccessResponse(response));
    }

    /// <summary>
    /// Registra uma nova conta via query string.
    /// </summary>
    /// <remarks>
    /// Exemplo:
    ///
    ///     POST /api/account/register/query?email=sam@test.com&amp;password=password123&amp;displayName=Sam
    /// </remarks>
    [SwaggerOperation(Summary = "Registrar conta por query", Description = "Cria uma nova conta recebendo os campos por query string.")]
    [ProducesCreatedApiResponse(typeof(AccountResponse))]
    [HttpPost("register/query")]
    public async Task<IActionResult> CreateAccountByQuery([FromQuery] CreateAccountInput input)
    {
        var user = await _createAccountUseCase.ExecuteAsync(input);
        var response = user.ToAccountResponse();
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<AccountResponse>.SuccessResponse(response));
    }

    /// <summary>
    /// Efetua login do usuário.
    /// </summary>
    /// <remarks>
    /// Exemplo de body:
    ///
    ///     POST /api/account/login
    ///     {
    ///       "email": "sam@test.com",
    ///       "password": "password123"
    ///     }
    /// </remarks>
    [SwaggerOperation(Summary = "Login", Description = "Autentica um usuário válido e retorna os dados públicos da conta.")]
    [Consumes("application/json")]
    [ProducesOkApiResponse(typeof(AccountResponse))]
    [ProducesUnauthorizedApiResponse]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginInput input)
    {
        var user = await _loginUseCase.ExecuteAsync(input);
        var response = user.ToAccountResponse();
        return Ok(ApiResponse<AccountResponse>.SuccessResponse(response));
    }
}
