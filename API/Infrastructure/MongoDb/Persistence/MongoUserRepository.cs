using API.Application.Ports.Persistence;
using API.Core.Entities;
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

    public async Task<AppUser?> GetByIdAsync(string id)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        return await _users.Find(Builders<AppUser>.Filter.Empty).ToListAsync();
    }

    public async Task AddAsync(AppUser user)
    {
        await _users.InsertOneAsync(user);
    }

    public async Task UpdateAsync(AppUser user)
    {
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
    }

    public async Task RemoveAsync(string id)
    {
        await _users.DeleteOneAsync(u => u.Id == id);
    }
}


