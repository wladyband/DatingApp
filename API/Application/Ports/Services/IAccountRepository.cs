using API.Domain.Entities;

namespace API.Application.Ports.Services;

public interface IAccountRepository
{
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(AppUser user, CancellationToken cancellationToken = default);
}
