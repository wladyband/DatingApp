namespace API.Infrastructure;

public interface IMongoDbInitializer
{
    Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger);
}