namespace API.Infrastructure;

public interface IPostgreSqlInitializer
{
    Task InitializeIfEnabledAsync(IServiceProvider services, IConfiguration configuration, ILogger logger);
}