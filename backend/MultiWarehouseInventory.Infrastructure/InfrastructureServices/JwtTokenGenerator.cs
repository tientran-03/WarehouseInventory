using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var secret = _configuration["JwtSettings:Secret"]
            ?? _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT Secret chưa được cấu hình. Thêm JwtSettings:Secret vào appsettings.json (tối thiểu 32 ký tự).");

        if (Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException(
                "JWT Secret quá ngắn. HS256 yêu cầu secret tối thiểu 32 ký tự (256 bit).");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var issuer = _configuration["JwtSettings:Issuer"] ?? _configuration["Jwt:Issuer"];
        var audience = _configuration["JwtSettings:Audience"] ?? _configuration["Jwt:Audience"];
        var expiryMinutes = int.TryParse(
            _configuration["JwtSettings:ExpiryMinutes"] ?? _configuration["Jwt:ExpiryMinutes"],
            out var minutes) ? minutes : 120;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim("tenantId", user.TenantId.Value.ToString()));
        }
        if (user.MustChangePassword)
        {
            claims.Add(new Claim("requires_password_change", "true"));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
