using API.Infrastructure.PostgreSql.Persistence;
using Npgsql;

namespace API.Infrastructure.PostgreSql;

public static class PostgreSqlInitializationExtensions
{
    public static async Task InitializePostgreSqlIfEnabledAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        var applyPostgreSqlMigrationsOnStartup =
            configuration.GetValue<bool>("PostgreSql:ApplyMigrationsOnStartup");

        if (!applyPostgreSqlMigrationsOnStartup)
        {
            logger.LogInformation("Migrações PostgreSQL desabilitadas por configuração.");
            return;
        }

        var dataSource = services.GetService<NpgsqlDataSource>();
        if (dataSource is null)
        {
            logger.LogDebug("PostgreSQL não configurado. Migrações não serão aplicadas.");
            return;
        }

        var environment = services.GetRequiredService<IWebHostEnvironment>();
        await PostgreSqlMigrationRunner.ApplyPendingMigrationsAsync(dataSource, environment, logger);
    }
}
