using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Application.Ports.Infrastructure;
using API.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace API.Domain.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(AppUser user)
    {
        var tokenKey = config["TokenKey"];
        if (string.IsNullOrWhiteSpace(tokenKey))
            throw new InvalidOperationException("TokenKey nao configurada. Defina em User Secrets (dev) ou variavel de ambiente.");

        if (tokenKey.Length < 64)
            throw new InvalidOperationException("TokenKey invalida. O valor deve ter pelo menos 64 caracteres.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id)
        };
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

}
