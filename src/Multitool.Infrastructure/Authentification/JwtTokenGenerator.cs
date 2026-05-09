
using Microsoft.Extensions.Configuration;
using Multitool.Domain.Entities.Config;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Multitool.Domain.Exceptions;

namespace Multitool.Infrastructure.Authentification;
public class JwtTokenGenerator(IConfiguration config) : IJwtTokenGenerator
{
    public string GenerateToken(User user)
    {
        var jwtSettings = config.GetSection("Jwt");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var key = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? throw new JwtMissingException("JWT_KEY missing in token generation");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}