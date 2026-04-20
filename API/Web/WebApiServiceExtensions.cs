using API.Web.ExceptionHandling;
using Microsoft.AspNetCore.Mvc;

namespace API.Web;

/// <summary>
/// Extension para registrar configuracoes HTTP da camada Web.
/// </summary>
public static class WebApiServiceExtensions
{
    public static IServiceCollection AddWebApi(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            // Registra o exception filter globalizado.
            options.Filters.Add<ApiExceptionFilter>();
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var details = new
                {
                    traceId = context.HttpContext.TraceIdentifier,
                    method = context.HttpContext.Request.Method,
                    path = context.HttpContext.Request.Path.Value,
                    errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e =>
                                string.IsNullOrWhiteSpace(e.ErrorMessage)
                                    ? "Valor inválido."
                                    : e.ErrorMessage).ToArray())
                };

                return new BadRequestObjectResult(
                    ApiResponse.ErrorResponse("Erro de validação na requisição.", "VALIDATION_ERROR", details));
            };
        });

        return services;
    }
}