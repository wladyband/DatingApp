namespace API.Infrastructure.PostgreSql.Configuration;

/// <summary>
/// Configuração global de persistência para escolher qual provider usar.
/// Localizada em PostgreSql/Configuration por estar relacionada ao conceito de "qual database é ativo".
/// </summary>
public class PersistenceOptions
{
    public const string SectionName = "Persistence";

    /// <summary>
    /// Provider ativo: "MongoDb" ou "PostgreSql"
    /// Default: "MongoDb" (ativo em desenvolvimento)
    /// </summary>
    public string Provider { get; set; } = "MongoDb";
}
