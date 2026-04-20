using Microsoft.OpenApi;
using System.Reflection;

namespace API.Web;

/// <summary>
/// Extension para registrar e configurar o Swagger/OpenAPI no pipeline da aplicação.
/// </summary>
public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DatingApp API",
                Version = "v1",
                Description = "API de contas e usuários do projeto DatingApp."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            options.EnableAnnotations();
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "DatingApp API v1");
                options.RoutePrefix = "swagger";
                options.DocumentTitle = "DatingApp API Docs";
            });
        }

        return app;
    }
}
