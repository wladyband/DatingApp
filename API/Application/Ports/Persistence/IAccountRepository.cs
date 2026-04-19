using API.Domain.Entities;

namespace API.Application.Ports.Persistence;

public interface IAccountRepository
{
    Task<AppUser?> GetByEmailAsync(string email);

    Task AddAsync(AppUser user);
}



