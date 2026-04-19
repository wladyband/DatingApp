using API.Application.Ports;
using API.Infrastructure.MongoDb.Persistence;

namespace API.Infrastructure.MongoDb.Accounts;

public static class MongoDbAccountsModuleExtensions
{
    public static IServiceCollection AddMongoDbAccountsModule(this IServiceCollection services)
    {
        services.AddScoped<MongoAccountRepository>();

        services.AddScoped<IAccountRepository>(sp =>
            sp.GetRequiredService<MongoAccountRepository>());

        return services;
    }
}
