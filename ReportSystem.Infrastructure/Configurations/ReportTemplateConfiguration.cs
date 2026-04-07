using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Configurations;

public class ReportTemplateConfiguration : IEntityTypeConfiguration<ReportTemplate>
{
    public void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        builder.ToTable("report_templates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TemplateCode)
            .HasColumnName("template_code")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.TemplateName)
            .HasColumnName("template_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(x => x.TemplateCode)
            .IsUnique();
    }
}
