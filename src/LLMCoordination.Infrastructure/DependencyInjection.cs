using System.Text;
using LLMCoordination.Application.Abstractions;
using LLMCoordination.Infrastructure.Data;
using LLMCoordination.Infrastructure.Mcp;
using LLMCoordination.Infrastructure.Repositories;
using LLMCoordination.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace LLMCoordination.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseInMemoryDatabase("LLMCoordination_Dev");
            }
            else
            {
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();
        services.AddScoped<ISwaggerRepository, SwaggerRepository>();
        services.AddScoped<IMcpRepository, McpRepository>();
        services.AddScoped<IDatabaseRepository, DatabaseRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

        services.AddSingleton<CredentialEncryptionService>();
        services.AddHttpClient(nameof(McpClient));
        services.AddScoped<McpClient>();

        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured.");
        var issuer = jwtSection["Issuer"] ?? "LLMCoordination";
        var audience = jwtSection["Audience"] ?? "LLMCoordination";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
                    RoleClaimType = "role"
                };
            });

        services.AddAuthorization();

        return services;
    }
}
