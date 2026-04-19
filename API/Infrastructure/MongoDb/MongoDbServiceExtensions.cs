using API.Infrastructure.MongoDb.Accounts;
using API.Infrastructure.MongoDb.Configuration;
using API.Infrastructure.MongoDb.Users;
using MongoDB.Driver;

namespace API.Infrastructure.MongoDb;

public static class MongoDbServiceExtensions
{
    public static IServiceCollection AddMongoDbServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddMongoDbCore(configuration)
            .AddMongoDbRepositories();
    }

    public static IServiceCollection AddMongoDbCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mongoOptions = configuration
            .GetSection(MongoDbOptions.SectionName)
            .Get<MongoDbOptions>() ?? new MongoDbOptions();

        if (string.IsNullOrWhiteSpace(mongoOptions.ConnectionString))
            throw new InvalidOperationException("MongoDb:ConnectionString não foi configurada.");

        if (string.IsNullOrWhiteSpace(mongoOptions.Database))
            throw new InvalidOperationException("MongoDb:Database não foi configurada.");

        services.RegisterMongoDbMappings();

        services.AddSingleton<IMongoClient>(_ =>
            new MongoClient(mongoOptions.ConnectionString));

        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoOptions.Database);
        });

        return services;
    }

    public static IServiceCollection AddMongoDbRepositories(this IServiceCollection services)
    {
        return services
            .AddMongoDbUsersModule()
            .AddMongoDbAccountsModule();
    }
}
