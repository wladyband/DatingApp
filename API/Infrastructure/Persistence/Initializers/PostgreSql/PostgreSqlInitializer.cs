using API.Infrastructure.PostgreSql;

namespace API.Infrastructure;

public sealed class PostgreSqlInitializer : IPostgreSqlInitializer
{
    public Task InitializeIfEnabledAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        return services.InitializePostgreSqlIfEnabledAsync(configuration, logger);
    }
}