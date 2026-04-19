using API.Core.Entities;

namespace API.Application.Ports;

public interface IAccountRepository
{
    Task<AppUser?> GetByEmailAsync(string email);

    Task AddAsync(AppUser user);
}
