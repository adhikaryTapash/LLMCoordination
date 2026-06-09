using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LLMCoordination.Application.Abstractions;
using LLMCoordination.Application.Contracts;
using LLMCoordination.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LLMCoordination.Application.Services;

public class AuthService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IConfiguration _configuration;

    public AuthService(ITenantRepository tenantRepository, IConfiguration configuration)
    {
        _tenantRepository = tenantRepository;
        _configuration = configuration;
    }

    public async Task<RegisterTenantResponse> RegisterTenantAsync(
        RegisterTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.TenantName,
            DomainType = request.DomainType,
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _tenantRepository.CreateAsync(tenant, cancellationToken);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            FullName = request.AdminFullName,
            Email = request.AdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
            Role = "Admin",
            Status = "Active"
        };

        await _tenantRepository.CreateUserAsync(user, cancellationToken);

        var token = GenerateToken(user);

        return new RegisterTenantResponse
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            Token = token
        };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _tenantRepository.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var expiryMinutes = _configuration.GetValue("Jwt:ExpiryMinutes", 60);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        return new LoginResponse
        {
            Token = GenerateToken(user),
            ExpiresAt = expiresAt,
            UserId = user.Id,
            TenantId = user.TenantId,
            Role = user.Role,
            FullName = user.FullName
        };
    }

    private string GenerateToken(AppUser user)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var issuer = _configuration["Jwt:Issuer"] ?? "LLMCoordination";
        var audience = _configuration["Jwt:Audience"] ?? "LLMCoordination";
        var expiryMinutes = _configuration.GetValue("Jwt:ExpiryMinutes", 60);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim("role", user.Role),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
