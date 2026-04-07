using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Configurations;

public class TemplateFieldConfiguration : IEntityTypeConfiguration<TemplateField>
{
    public void Configure(EntityTypeBuilder<TemplateField> builder)
    {
        builder.ToTable("template_fields");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TemplateVersionId)
            .HasColumnName("template_version_id")
            .IsRequired();

        builder.Property(x => x.FieldCode)
            .HasColumnName("field_code")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.FieldLabel)
            .HasColumnName("field_label")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.DataType)
            .HasColumnName("data_type")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasColumnName("unit")
            .HasMaxLength(50);

        builder.Property(x => x.IsRequired)
            .HasColumnName("is_required")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired();

        builder.Property(x => x.Placeholder)
            .HasColumnName("placeholder")
            .HasMaxLength(255);

        builder.Property(x => x.OptionsJson)
            .HasColumnName("options_json")
            .HasColumnType("nvarchar(max)");

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

        builder.HasIndex(x => new { x.TemplateVersionId, x.FieldCode })
            .IsUnique();

        builder.HasIndex(x => new { x.TemplateVersionId, x.DisplayOrder })
            .IsUnique();

        builder.HasOne(x => x.TemplateVersion)
            .WithMany(x => x.Fields)
            .HasForeignKey(x => x.TemplateVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
