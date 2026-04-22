namespace API.Infrastructure.MongoDb.DependencyInjection;

public static class MongoDbInfrastructureRegistrationExtensions
{
    public static IServiceCollection AddRequiredMongoDbInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddMongoDbServices(configuration);
    }
}