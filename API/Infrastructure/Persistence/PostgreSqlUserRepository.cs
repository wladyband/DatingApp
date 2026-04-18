using API.Application.Ports;
using API.Core.Entities;
using Npgsql;

namespace API.Infrastructure.Persistence;

/// <summary>
/// Adapter de saída para persistência de usuários no PostgreSQL via Npgsql.
/// Implementa exatamente o mesmo contrato da camada Application.
/// </summary>
public class PostgreSqlUserRepository : IUserRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgreSqlUserRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<AppUser?> GetByIdAsync(string id)
    {
        const string sql = "SELECT \"Id\", \"Displayname\", \"Email\" FROM \"Users\" WHERE \"Id\" = @id LIMIT 1";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapUser(reader);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        const string sql = "SELECT \"Id\", \"Displayname\", \"Email\" FROM \"Users\" WHERE \"Email\" = @email LIMIT 1";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("email", email);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return MapUser(reader);
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        const string sql = "SELECT \"Id\", \"Displayname\", \"Email\" FROM \"Users\"";

        var users = new List<AppUser>();

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapUser(reader));
        }

        return users;
    }

    public async Task AddAsync(AppUser user)
    {
        const string sql = "INSERT INTO \"Users\" (\"Id\", \"Displayname\", \"Email\") VALUES (@id, @displayname, @email)";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("displayname", user.Displayname);
        command.Parameters.AddWithValue("email", user.Email);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(AppUser user)
    {
        const string sql = "UPDATE \"Users\" SET \"Displayname\" = @displayname, \"Email\" = @email WHERE \"Id\" = @id";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("displayname", user.Displayname);
        command.Parameters.AddWithValue("email", user.Email);
        await command.ExecuteNonQueryAsync();
    }

    public async Task RemoveAsync(string id)
    {
        const string sql = "DELETE FROM \"Users\" WHERE \"Id\" = @id";

        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync();
    }

    private static AppUser MapUser(NpgsqlDataReader reader)
    {
        return new AppUser
        {
            Id = reader.GetString(0),
            Displayname = reader.GetString(1),
            Email = reader.GetString(2)
        };
    }
}
