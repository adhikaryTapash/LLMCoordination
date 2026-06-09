using LLMCoordination.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LLMCoordination.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<TenantSwaggerDocument> TenantSwaggerDocuments => Set<TenantSwaggerDocument>();
    public DbSet<TenantSwaggerEndpoint> TenantSwaggerEndpoints => Set<TenantSwaggerEndpoint>();
    public DbSet<TenantMcpServer> TenantMcpServers => Set<TenantMcpServer>();
    public DbSet<TenantMcpTool> TenantMcpTools => Set<TenantMcpTool>();
    public DbSet<TenantDatabaseConnection> TenantDatabaseConnections => Set<TenantDatabaseConnection>();
    public DbSet<TenantDatabaseSchema> TenantDatabaseSchemas => Set<TenantDatabaseSchema>();
    public DbSet<GlobalSkill> GlobalSkills => Set<GlobalSkill>();
    public DbSet<GlobalSkillIntentExample> GlobalSkillIntentExamples => Set<GlobalSkillIntentExample>();
    public DbSet<GlobalSkillPrompt> GlobalSkillPrompts => Set<GlobalSkillPrompt>();
    public DbSet<GlobalSkillResponseSchema> GlobalSkillResponseSchemas => Set<GlobalSkillResponseSchema>();
    public DbSet<TenantSkillEndpointMapping> TenantSkillEndpointMappings => Set<TenantSkillEndpointMapping>();
    public DbSet<TenantSkillMcpMapping> TenantSkillMcpMappings => Set<TenantSkillMcpMapping>();
    public DbSet<TenantPermissionRule> TenantPermissionRules => Set<TenantPermissionRule>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<AgentAuditLog> AgentAuditLogs => Set<AgentAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTenant(modelBuilder);
        ConfigureAppUser(modelBuilder);
        ConfigureSwagger(modelBuilder);
        ConfigureMcp(modelBuilder);
        ConfigureDatabase(modelBuilder);
        ConfigureGlobalSkills(modelBuilder);
        ConfigureMappings(modelBuilder);
        ConfigureChatAndAudit(modelBuilder);
    }

    private static void ConfigureTenant(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }

    private static void ConfigureAppUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSwagger(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantSwaggerDocument>(entity =>
        {
            entity.ToTable("tenant_swagger_documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DocumentUrl).HasMaxLength(2000);
            entity.Property(e => e.Version).HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TenantId);
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.SwaggerDocuments)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TenantSwaggerEndpoint>(entity =>
        {
            entity.ToTable("tenant_swagger_endpoints");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Method).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OperationId).HasMaxLength(200);
            entity.Property(e => e.Summary).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.SwaggerDocumentId);
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.SwaggerDocument)
                .WithMany(d => d.Endpoints)
                .HasForeignKey(e => e.SwaggerDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureMcp(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantMcpServer>(entity =>
        {
            entity.ToTable("tenant_mcp_servers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BaseUrl).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TenantId);
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.McpServers)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TenantMcpTool>(entity =>
        {
            entity.ToTable("tenant_mcp_tools");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ToolName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.McpServerId);
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.McpServer)
                .WithMany(s => s.Tools)
                .HasForeignKey(e => e.McpServerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDatabase(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantDatabaseConnection>(entity =>
        {
            entity.ToTable("tenant_database_connections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DbType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Host).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DatabaseName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EncryptedConnectionString).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TenantId);
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.DatabaseConnections)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TenantDatabaseSchema>(entity =>
        {
            entity.ToTable("tenant_database_schema");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TableName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ColumnName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DataType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.ConnectionId);
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Connection)
                .WithMany(c => c.SchemaEntries)
                .HasForeignKey(e => e.ConnectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureGlobalSkills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GlobalSkill>(entity =>
        {
            entity.ToTable("global_skill_master");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SkillName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.SkillName).IsUnique();
        });

        modelBuilder.Entity<GlobalSkillIntentExample>(entity =>
        {
            entity.ToTable("global_skill_intent_examples");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExampleText).IsRequired().HasMaxLength(1000);
            entity.HasIndex(e => e.SkillId);
            entity.HasOne(e => e.Skill)
                .WithMany(s => s.IntentExamples)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GlobalSkillPrompt>(entity =>
        {
            entity.ToTable("global_skill_prompt");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PromptTemplate).IsRequired();
            entity.HasIndex(e => e.SkillId);
            entity.HasOne(e => e.Skill)
                .WithMany(s => s.Prompts)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GlobalSkillResponseSchema>(entity =>
        {
            entity.ToTable("global_skill_response_schema");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ResponseSchemaJson).IsRequired();
            entity.HasIndex(e => e.SkillId);
            entity.HasOne(e => e.Skill)
                .WithMany(s => s.ResponseSchemas)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureMappings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantSkillEndpointMapping>(entity =>
        {
            entity.ToTable("tenant_skill_endpoint_mapping");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.SkillId, e.EndpointId }).IsUnique();
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Skill)
                .WithMany(s => s.EndpointMappings)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Endpoint)
                .WithMany(ep => ep.SkillMappings)
                .HasForeignKey(e => e.EndpointId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TenantSkillMcpMapping>(entity =>
        {
            entity.ToTable("tenant_skill_mcp_mapping");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.SkillId, e.McpToolId }).IsUnique();
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Skill)
                .WithMany(s => s.McpMappings)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.McpTool)
                .WithMany(t => t.SkillMappings)
                .HasForeignKey(e => e.McpToolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TenantPermissionRule>(entity =>
        {
            entity.ToTable("tenant_permission_rules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Resource).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.Role, e.Resource, e.Action }).IsUnique();
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.PermissionRules)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureChatAndAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Conversations)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Conversations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ConversationMessage>(entity =>
        {
            entity.ToTable("conversation_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MessageText).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.ConversationId);
            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AgentAuditLog>(entity =>
        {
            entity.ToTable("agent_audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AgentName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SkillName).HasMaxLength(200);
            entity.Property(e => e.ToolName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => e.ConversationId);
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
