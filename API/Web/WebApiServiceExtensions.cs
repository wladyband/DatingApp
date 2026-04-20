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
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Title = "Erro de validação.",
                    Status = StatusCodes.Status400BadRequest
                };

                return new BadRequestObjectResult(problemDetails);
            };
        });

        return services;
    }
}