using API.Application.Ports;
using API.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Persistence;

/// <summary>
/// Adapter de saída que implementa o port IUserRepository usando Entity Framework Core.
/// Esta é a implementação concreta de como os usuários são persistidos no banco de dados SQLite.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByIdAsync(string id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task AddAsync(AppUser user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task UpdateAsync(AppUser user)
    {
        _context.Users.Update(user);
    }

    public async Task RemoveAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
