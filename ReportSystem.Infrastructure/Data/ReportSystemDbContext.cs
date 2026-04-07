using Microsoft.EntityFrameworkCore;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Data;

public class ReportSystemDbContext : DbContext
{
    public ReportSystemDbContext(DbContextOptions<ReportSystemDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<ReportTemplate> ReportTemplates => Set<ReportTemplate>();

    public DbSet<ReportTemplateVersion> ReportTemplateVersions => Set<ReportTemplateVersion>();

    public DbSet<TemplateField> TemplateFields => Set<TemplateField>();

    public DbSet<FieldRule> FieldRules => Set<FieldRule>();

    public DbSet<ReportSubmission> ReportSubmissions => Set<ReportSubmission>();

    public DbSet<ReportFieldValue> ReportFieldValues => Set<ReportFieldValue>();

    public DbSet<ReportAttachment> ReportAttachments => Set<ReportAttachment>();

    public DbSet<ApprovalLog> ApprovalLogs => Set<ApprovalLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportSystemDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

}

