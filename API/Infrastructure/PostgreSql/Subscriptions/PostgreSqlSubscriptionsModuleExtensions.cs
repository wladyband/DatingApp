namespace API.Infrastructure.PostgreSql.Subscriptions;

public static class PostgreSqlSubscriptionsModuleExtensions
{
    public static IServiceCollection AddPostgreSqlSubscriptionsModule(this IServiceCollection services)
    {
        // Repositórios do domínio de assinaturas serão registrados aqui
        // quando o módulo PostgreSQL for implementado.
        return services;
    }
}
