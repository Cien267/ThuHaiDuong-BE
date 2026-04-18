using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Domain.Entities;

namespace ThuHaiDuong.Application.ImplementService;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
 
    public JwtService(IConfiguration config)
    {
        _config = config;
    }
 
    public string GenerateAccessToken(User user)
    {
        var key         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
 
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Name,           user.UserName),
            new Claim(ClaimTypes.Role,           user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
 
        var expiresMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60");
 
        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials);
 
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
 
    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
 
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler   = new JwtSecurityTokenHandler();
        var key            = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!);
 
        try
        {
            var principal = tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(key),
                    ValidateIssuer           = true,
                    ValidIssuer              = _config["Jwt:Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = _config["Jwt:Audience"],
                    ValidateLifetime         = false, // refresh token flow không check expire
                    ClockSkew                = TimeSpan.Zero,
                },
                out _);
 
            return principal;
        }
        catch
        {
            return null;
        }
    }
}