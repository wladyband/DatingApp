using API.Infrastructure.Configuration;
using API.Infrastructure.MongoDb;
using API.Infrastructure.PostgreSql;

namespace API.Infrastructure;

public static class InfrastructureInitializationExtensions
{
    public static async Task InitializePersistenceAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("PersistenceInitialization");

        var persistenceOptions = configuration
            .GetSection(PersistenceOptions.SectionName)
            .Get<PersistenceOptions>() ?? new PersistenceOptions();

        // PostgreSQL: migrações só rodam quando habilitadas explicitamente por configuração.
        // Isso evita qualquer alteração acidental no banco PostgreSQL enquanto o domínio
        // de assinaturas ainda não estiver em desenvolvimento.
        await services.InitializePostgreSqlIfEnabledAsync(configuration, logger);

        // Seed de dados pertence exclusivamente ao MongoDB (usuários e exercícios).
        // PostgreSQL não recebe seed neste momento.
        if (!string.Equals(persistenceOptions.Provider, "MongoDb", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await services.InitializeMongoDbAsync(configuration, logger);
    }
}