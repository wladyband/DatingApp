using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using API.Core.Exceptions;

namespace API.Infrastructure.Http.Filters;

/// <summary>
/// Exception filter centralizado para tratar todas as exceções de domínio.
/// Converte Domain Exceptions em respostas HTTP apropriadas.
/// </summary>
public class ApiExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;

        _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);

        var response = exception switch
        {
            // Domain Exceptions: erro de negócio esperado
            UserAlreadyExistsException uae =>
                new ObjectResult(ApiResponse.ErrorResponse(uae.Message, "USER_ALREADY_EXISTS"))
                {
                    StatusCode = StatusCodes.Status409Conflict
                },

            InvalidCredentialsException =>
                new ObjectResult(ApiResponse.ErrorResponse(exception.Message, "INVALID_CREDENTIALS"))
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                },

            DomainException de =>
                new ObjectResult(ApiResponse.ErrorResponse(de.Message, "DOMAIN_ERROR"))
                {
                    StatusCode = StatusCodes.Status400BadRequest
                },

            ArgumentException ae =>
                new ObjectResult(ApiResponse.ErrorResponse(ae.Message, "INVALID_ARGUMENT"))
                {
                    StatusCode = StatusCodes.Status400BadRequest
                },

            // Fallback: erro técnico não tratado
            _ =>
                new ObjectResult(ApiResponse.ErrorResponse(
                    "Erro interno do servidor. Tente novamente mais tarde.",
                    "INTERNAL_SERVER_ERROR"))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
        };

        context.Result = response;
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
