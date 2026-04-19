using API.Application.Ports;
using API.Infrastructure.MongoDb.Persistence;

namespace API.Infrastructure.MongoDb.Users;

public static class MongoDbUsersModuleExtensions
{
    public static IServiceCollection AddMongoDbUsersModule(this IServiceCollection services)
    {
        services.AddScoped<MongoUserRepository>();

        services.AddScoped<IUserRepository>(sp =>
            sp.GetRequiredService<MongoUserRepository>());

        return services;
    }
}
