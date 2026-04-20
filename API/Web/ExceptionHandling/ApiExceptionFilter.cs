using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using API.Web;
using API.Domain.Exceptions;

namespace API.Web.ExceptionHandling;

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
        var httpContext = context.HttpContext;

        var details = new
        {
            traceId = httpContext.TraceIdentifier,
            method = httpContext.Request.Method,
            path = httpContext.Request.Path.Value
        };

        _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);

        var response = exception switch
        {
            // Domain Exceptions: erro de negócio esperado
            UserAlreadyExistsException uae =>
                new ObjectResult(ApiResponse.ErrorResponse(uae.Message, "USER_ALREADY_EXISTS", details))
                {
                    StatusCode = StatusCodes.Status409Conflict
                },

            InvalidCredentialsException =>
                new ObjectResult(ApiResponse.ErrorResponse(exception.Message, "INVALID_CREDENTIALS", details))
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                },

            DomainException de =>
                new ObjectResult(ApiResponse.ErrorResponse(de.Message, "DOMAIN_ERROR", details))
                {
                    StatusCode = StatusCodes.Status400BadRequest
                },

            ArgumentException ae =>
                new ObjectResult(ApiResponse.ErrorResponse(ae.Message, "INVALID_ARGUMENT", details))
                {
                    StatusCode = StatusCodes.Status400BadRequest
                },

            // Fallback: erro técnico não tratado
            _ =>
                new ObjectResult(ApiResponse.ErrorResponse(
                    "Erro interno do servidor. Tente novamente mais tarde.",
                    "INTERNAL_SERVER_ERROR",
                    details))
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                }
        };

        context.Result = response;
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }
}
