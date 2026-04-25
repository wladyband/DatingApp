using API.Application.Ports.Services;
using API.Domain.Entities;
using API.Infrastructure.MongoDb.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API.Infrastructure.MongoDb.Persistence;

/// <summary>
/// Adapter de saída para persistência de usuários no MongoDB.
/// Implementa exatamente o mesmo contrato da camada Application.
/// </summary>
public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<AppUser> _users;

    public MongoUserRepository(IMongoDatabase database, IOptions<MongoDbOptions> options)
    {
        _users = database.GetCollection<AppUser>(options.Value.UsersCollection);
    }

    public async Task<AppUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _users.Find(Builders<AppUser>.Filter.Empty).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        await _users.InsertOneAsync(user, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: cancellationToken);
    }

    public async Task RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        await _users.DeleteOneAsync(u => u.Id == id, cancellationToken);
    }
}


