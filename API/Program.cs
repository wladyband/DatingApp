using API.Application;
using API.Infrastructure;
using API.Web.ExceptionHandling;
using API.Infrastructure.External;
using API.Application.Ports.External;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Registra os serviços de entrada HTTP (controllers).
builder.Services.AddControllers(options =>
{
    // Registra o exception filter globalizado
    options.Filters.Add<ApiExceptionFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // Coloque breakpoint aqui para depurar erros de validação automática (400).
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Title = "Erro de validação.",
            Status = StatusCodes.Status400BadRequest
        };

        return new BadRequestObjectResult(problemDetails);
    };
});

// Habilita CORS para permitir chamadas do frontend Angular local.
builder.Services.AddCors();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
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

// Registra casos de uso e serviços da camada Application.
builder.Services.AddApplicationServices();

// Registra adaptadores técnicos (persistencia, banco e implementacoes concretas).
builder.Services.AddInfrastructureServices(builder.Configuration);

// Registra serviços de infraestrutura (email, logger, etc)
builder.Services.AddScoped<IEmailPortAdapter, EmailPortAdapter>();
builder.Services.AddScoped<ILoggerPort, LoggerPortAdapter>();

// Opcional: habilitar OpenAPI quando quiser expor Swagger.
//builder.Services.AddOpenApi();

var app = builder.Build();

await app.InitializePersistenceAsync();

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

// Politica CORS ativa para origem do frontend em ambiente local.
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod()
    .WithOrigins("http://localhost:4200", "https://localhost:4200"));

// Mapeia os endpoints dos controllers da API.
app.MapControllers();

// Inicia a aplicacao web.
app.Run();


