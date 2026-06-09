using LLMCoordination.Domain.Entities;
using LLMCoordination.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LLMCoordination.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);

        await SeedGlobalSkillsAsync(context, cancellationToken);
        await SeedDemoTenantAsync(context, logger, cancellationToken);
    }

    private static async Task SeedGlobalSkillsAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.GlobalSkills.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        foreach (var definition in SkillSeedData.Skills)
        {
            var skill = new GlobalSkill
            {
                Id = Guid.NewGuid(),
                SkillName = definition.SkillName,
                Domain = definition.Domain,
                Category = definition.Category,
                Description = definition.Description,
                ActionType = definition.ActionType,
                Enabled = true,
                CreatedAt = now
            };

            context.GlobalSkills.Add(skill);

            if (definition.IntentExamples is not null)
            {
                foreach (var example in definition.IntentExamples)
                {
                    context.GlobalSkillIntentExamples.Add(new GlobalSkillIntentExample
                    {
                        Id = Guid.NewGuid(),
                        SkillId = skill.Id,
                        ExampleText = example
                    });
                }
            }

            context.GlobalSkillPrompts.Add(new GlobalSkillPrompt
            {
                Id = Guid.NewGuid(),
                SkillId = skill.Id,
                PromptTemplate = SkillSeedData.DefaultPromptTemplate.Replace("{{skillName}}", definition.SkillName),
                Version = 1,
                Active = true
            });

            context.GlobalSkillResponseSchemas.Add(new GlobalSkillResponseSchema
            {
                Id = Guid.NewGuid(),
                SkillId = skill.Id,
                ResponseSchemaJson = SkillSeedData.DefaultResponseSchemaJson,
                Version = 1,
                Active = true
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDemoTenantAsync(AppDbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        const string demoEmail = "admin@demo.local";

        if (await context.AppUsers.AnyAsync(u => u.Email == demoEmail, cancellationToken))
        {
            return;
        }

        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Demo Healthcare",
            DomainType = DomainType.Healthcare,
            Status = "Active",
            CreatedAt = now
        };

        var adminUser = new AppUser
        {
            Id = userId,
            TenantId = tenantId,
            FullName = "Demo Admin",
            Email = demoEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
            Role = "Admin",
            Status = "Active",
            CreatedAt = now
        };

        context.Tenants.Add(tenant);
        context.AppUsers.Add(adminUser);
        context.TenantPermissionRules.AddRange(CreateDefaultPermissionRules(tenantId));

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded demo tenant {TenantName} with admin {Email}", tenant.Name, demoEmail);
    }

    public static IReadOnlyList<TenantPermissionRule> CreateDefaultPermissionRules(Guid tenantId)
    {
        return
        [
            // Admin — full access
            Rule(tenantId, "Admin", "*", "Read", true),
            Rule(tenantId, "Admin", "*", "Create", true),
            Rule(tenantId, "Admin", "*", "Update", true),
            Rule(tenantId, "Admin", "*", "Delete", true),
            Rule(tenantId, "Admin", "*", "Execute", true),
            Rule(tenantId, "Admin", "*", "Export", true),

            // User — read + execute, writes require approval (Allowed=false blocks direct execution)
            Rule(tenantId, "User", "*", "Read", true),
            Rule(tenantId, "User", "*", "Create", false),
            Rule(tenantId, "User", "*", "Update", false),
            Rule(tenantId, "User", "*", "Delete", false),
            Rule(tenantId, "User", "*", "Execute", true),
            Rule(tenantId, "User", "*", "Export", false),

            // Viewer — read only
            Rule(tenantId, "Viewer", "*", "Read", true),
            Rule(tenantId, "Viewer", "*", "Create", false),
            Rule(tenantId, "Viewer", "*", "Update", false),
            Rule(tenantId, "Viewer", "*", "Delete", false),
            Rule(tenantId, "Viewer", "*", "Execute", false),
            Rule(tenantId, "Viewer", "*", "Export", false)
        ];
    }

    private static TenantPermissionRule Rule(Guid tenantId, string role, string resource, string action, bool allowed) =>
        new()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Role = role,
            Resource = resource,
            Action = action,
            Allowed = allowed
        };
}
