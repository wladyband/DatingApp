using API.Application;
using API.Infrastructure;
using API.Infrastructure.Http.Filters;
using API.Infrastructure.External;
using API.Application.Ports.External;

var builder = WebApplication.CreateBuilder(args);

// Registra os serviços de entrada HTTP (controllers).
builder.Services.AddControllers(options =>
{
    // Registra o exception filter globalizado
    options.Filters.Add<ApiExceptionFilter>();
});

// Habilita CORS para permitir chamadas do frontend Angular local.
builder.Services.AddCors();

// Registra casos de uso e serviços da camada Application.
builder.Services.AddApplicationServices();

// Registra adaptadores técnicos (persistencia, banco e implementacoes concretas).
builder.Services.AddInfrastructureServices(builder.Configuration);

// Registra serviços de infraestrutura (email, logger, etc)
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ILoggerPort, LoggerPortAdapter>();

// Opcional: habilitar OpenAPI quando quiser expor Swagger.
//builder.Services.AddOpenApi();

var app = builder.Build();

await app.InitializePersistenceAsync();

// Politica CORS ativa para origem do frontend em ambiente local.
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod()
    .WithOrigins("http://localhost:4200", "https://localhost:4200"));

// Mapeia os endpoints dos controllers da API.
app.MapControllers();

// Inicia a aplicacao web.
app.Run();


