using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Configurations;

public class FieldRuleConfiguration : IEntityTypeConfiguration<FieldRule>
{
    public void Configure(EntityTypeBuilder<FieldRule> builder)
    {
        builder.ToTable("field_rules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.FieldId)
            .HasColumnName("field_id")
            .IsRequired();

        builder.Property(x => x.RuleOrder)
            .HasColumnName("rule_order")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(x => x.RuleType)
            .HasColumnName("rule_type")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.MinValue)
            .HasColumnName("min_value")
            .HasPrecision(18, 6);

        builder.Property(x => x.MaxValue)
            .HasColumnName("max_value")
            .HasPrecision(18, 6);

        builder.Property(x => x.ThresholdValue)
            .HasColumnName("threshold_value")
            .HasPrecision(18, 6);

        builder.Property(x => x.ExpectedText)
            .HasColumnName("expected_text")
            .HasMaxLength(500);

        builder.Property(x => x.Severity)
            .HasColumnName("severity")
            .HasMaxLength(20)
            .IsUnicode(false)
            .HasDefaultValue("ERROR")
            .IsRequired();

        builder.Property(x => x.FailMessage)
            .HasColumnName("fail_message")
            .HasMaxLength(500);

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

        builder.HasIndex(x => new { x.FieldId, x.RuleOrder })
            .IsUnique();

        builder.HasOne(x => x.Field)
            .WithMany(x => x.Rules)
            .HasForeignKey(x => x.FieldId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
