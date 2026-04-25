using API.Application.Ports.Services;
using API.Domain.Entities;
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

    public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        await _users.InsertOneAsync(user, cancellationToken: cancellationToken);
    }
}


