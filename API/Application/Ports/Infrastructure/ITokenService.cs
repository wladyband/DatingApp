using System;
using API.Domain.Entities;

namespace API.Application.Ports.Infrastructure;

public interface ITokenService
{
    string CreateToken(AppUser user);
}
