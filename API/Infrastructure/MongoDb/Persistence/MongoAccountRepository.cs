using API.Application.Ports;
using API.Core.Entities;
using API.Infrastructure.MongoDb.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API.Infrastructure.MongoDb.Persistence;

public class MongoAccountRepository : IAccountRepository
{
    private readonly IMongoCollection<AppUser> _users;

    public MongoAccountRepository(IMongoDatabase database, IOptions<MongoDbOptions> options)
    {
        _users = database.GetCollection<AppUser>(options.Value.UsersCollection);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task AddAsync(AppUser user)
    {
        await _users.InsertOneAsync(user);
    }
}
