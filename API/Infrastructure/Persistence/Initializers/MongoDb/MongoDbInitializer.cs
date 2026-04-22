using API.Infrastructure.MongoDb;

namespace API.Infrastructure;

public sealed class MongoDbInitializer : IMongoDbInitializer
{
    public Task InitializeAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
    {
        return services.InitializeMongoDbAsync(configuration, logger);
    }
}