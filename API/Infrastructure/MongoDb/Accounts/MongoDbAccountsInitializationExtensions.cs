namespace API.Infrastructure.MongoDb.Accounts;

public static class MongoDbAccountsInitializationExtensions
{
    public static Task InitializeMongoDbAccountsAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        // Placeholder para inicializações específicas de Accounts no MongoDB.
        return Task.CompletedTask;
    }
}
