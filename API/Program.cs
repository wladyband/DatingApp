using API.Application;
using API.Infrastructure;
using API.Infrastructure.External;
using API.Application.Ports.External;
using API.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi();

// Habilita CORS para permitir chamadas do frontend Angular local.
builder.Services.AddCors();

builder.Services.AddSwaggerDocumentation();

// Registra casos de uso e serviços da camada Application.
builder.Services.AddApplicationServices();

// Registra adaptadores técnicos (persistencia, banco e implementacoes concretas).
builder.Services.AddInfrastructureServices(builder.Configuration);

// Registra serviços de infraestrutura (email, logger, etc)
builder.Services.AddScoped<IEmailPortAdapter, EmailPortAdapter>();
builder.Services.AddScoped<ILoggerPort, LoggerPortAdapter>();


var app = builder.Build();

await app.InitializePersistenceAsync();

app.UseSwaggerDocumentation();

// Politica CORS ativa para origem do frontend em ambiente local.
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod()
    .WithOrigins("http://localhost:4200", "https://localhost:4200"));

// Mapeia os endpoints dos controllers da API.
app.MapControllers();

// Inicia a aplicacao web.
app.Run();


