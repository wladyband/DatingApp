using API.Application;
using API.Infrastructure;
using API.Infrastructure.External;
using API.Application.Ports.External;
using API.Web;
using API.Web.ExceptionHandling;
using API.Application.Ports.Infrastructure;
using API.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi();

// Habilita CORS para permitir chamadas do frontend Angular local.
builder.Services.AddCors();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenKey = builder.Configuration["TokenKey"]
            ?? throw new Exception("Token key not found - Program.cs");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(tokenKey)
            ),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });


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

// Garante payload de erro padrao para status sem corpo (ex.: 404/405/415).
app.UseApiStatusCodeResponses();
app.UseAuthentication();
app.UseAuthorization();

// Mapeia os endpoints dos controllers da API.
app.MapControllers();

// Inicia a aplicacao web.
app.Run();


