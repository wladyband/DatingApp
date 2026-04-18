using System.Text.RegularExpressions;
using Npgsql;

namespace API.Infrastructure.Persistence;

public static class PostgreSqlMigrationRunner
{
    private const string MigrationsHistoryTable = "__schema_migrations";

    public static async Task ApplyPendingMigrationsAsync(
        NpgsqlDataSource dataSource,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        var migrationsDirectory = Path.Combine(
            environment.ContentRootPath,
            "Infrastructure",
            "Persistence",
            "Migrations",
            "PostgreSql");

        if (!Directory.Exists(migrationsDirectory))
        {
            logger.LogWarning("Diretório de migrations PostgreSQL não encontrado: {Path}", migrationsDirectory);
            return;
        }

        await using var connection = await dataSource.OpenConnectionAsync();

        await EnsureHistoryTableAsync(connection);

        var allMigrationFiles = Directory
            .GetFiles(migrationsDirectory, "*.sql", SearchOption.TopDirectoryOnly)
            .OrderBy(Path.GetFileName)
            .ToList();

        var appliedMigrations = await GetAppliedMigrationsAsync(connection);

        foreach (var migrationFile in allMigrationFiles)
        {
            var fileName = Path.GetFileName(migrationFile);
            var migrationId = ExtractMigrationId(fileName);

            if (string.IsNullOrWhiteSpace(migrationId))
            {
                throw new InvalidOperationException(
                    $"Nome de migration inválido: '{fileName}'. Use o padrão '<timestamp>_<descricao>.sql'.");
            }

            if (appliedMigrations.Contains(migrationId))
            {
                continue;
            }

            var sql = await File.ReadAllTextAsync(migrationFile);
            if (string.IsNullOrWhiteSpace(sql))
            {
                logger.LogWarning("Migration vazia ignorada: {File}", fileName);
                await RegisterMigrationAsync(connection, migrationId);
                continue;
            }

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await using var command = new NpgsqlCommand(sql, connection, transaction);
                await command.ExecuteNonQueryAsync();

                await RegisterMigrationAsync(connection, migrationId, transaction);

                await transaction.CommitAsync();
                logger.LogInformation("Migration PostgreSQL aplicada: {Migration}", fileName);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    private static async Task EnsureHistoryTableAsync(NpgsqlConnection connection)
    {
        var sql = $"""
            CREATE TABLE IF NOT EXISTS "{MigrationsHistoryTable}" (
                "MigrationId" text PRIMARY KEY,
                "AppliedAtUtc" timestamptz NOT NULL
            );
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<HashSet<string>> GetAppliedMigrationsAsync(NpgsqlConnection connection)
    {
        var sql = $"SELECT \"MigrationId\" FROM \"{MigrationsHistoryTable}\";";
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync())
        {
            applied.Add(reader.GetString(0));
        }

        return applied;
    }

    private static async Task RegisterMigrationAsync(
        NpgsqlConnection connection,
        string migrationId,
        NpgsqlTransaction? transaction = null)
    {
        var sql = $"""
            INSERT INTO "{MigrationsHistoryTable}" ("MigrationId", "AppliedAtUtc")
            VALUES (@migrationId, NOW())
            ON CONFLICT ("MigrationId") DO NOTHING;
            """;

        await using var command = transaction is null
            ? new NpgsqlCommand(sql, connection)
            : new NpgsqlCommand(sql, connection, transaction);

        command.Parameters.AddWithValue("migrationId", migrationId);
        await command.ExecuteNonQueryAsync();
    }

    private static string ExtractMigrationId(string fileName)
    {
        var match = Regex.Match(fileName, "^(\\d+)_.*\\.sql$", RegexOptions.CultureInvariant);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
