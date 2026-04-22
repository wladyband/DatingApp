using API.Domain.Entities;

namespace API.Application.Ports.Services;

public interface IAccountRepository
{
    Task<AppUser?> GetByEmailAsync(string email);

    Task AddAsync(AppUser user);
}
