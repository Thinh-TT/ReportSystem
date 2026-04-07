using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Configurations;

public class ReportFieldValueConfiguration : IEntityTypeConfiguration<ReportFieldValue>
{
    public void Configure(EntityTypeBuilder<ReportFieldValue> builder)
    {
        builder.ToTable("report_field_values");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SubmissionId)
            .HasColumnName("submission_id")
            .IsRequired();

        builder.Property(x => x.FieldId)
            .HasColumnName("field_id")
            .IsRequired();

        builder.Property(x => x.ValueText)
            .HasColumnName("value_text")
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ValueNumber)
            .HasColumnName("value_number")
            .HasPrecision(18, 6);

        builder.Property(x => x.ValueDate)
            .HasColumnName("value_date")
            .HasColumnType("date");

        builder.Property(x => x.ValueDateTime)
            .HasColumnName("value_datetime")
            .HasColumnType("datetime2");

        builder.Property(x => x.ValueBool)
            .HasColumnName("value_bool");

        builder.Property(x => x.NormalizedValue)
            .HasColumnName("normalized_value")
            .HasMaxLength(255);

        builder.Property(x => x.AutoResult)
            .HasColumnName("auto_result")
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasDefaultValue("PENDING")
            .IsRequired();

        builder.Property(x => x.EvaluationNote)
            .HasColumnName("evaluation_note")
            .HasMaxLength(500);

        builder.Property(x => x.RuleSnapshotJson)
            .HasColumnName("rule_snapshot_json")
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(x => new { x.SubmissionId, x.FieldId })
            .IsUnique();

        builder.HasOne(x => x.Submission)
            .WithMany(x => x.FieldValues)
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Field)
            .WithMany(x => x.FieldValues)
            .HasForeignKey(x => x.FieldId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
