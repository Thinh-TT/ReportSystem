using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Configurations;

public class ReportSubmissionConfiguration : IEntityTypeConfiguration<ReportSubmission>
{
    public void Configure(EntityTypeBuilder<ReportSubmission> builder)
    {
        builder.ToTable("report_submissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SubmissionNo)
            .HasColumnName("submission_no")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.TemplateVersionId)
            .HasColumnName("template_version_id")
            .IsRequired();

        builder.Property(x => x.ReportDate)
            .HasColumnName("report_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(x => x.PerformedByText)
            .HasColumnName("performed_by_text")
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.AutoResult)
            .HasColumnName("auto_result")
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasDefaultValue("PENDING")
            .IsRequired();

        builder.Property(x => x.ManagerResult)
            .HasColumnName("manager_result")
            .HasMaxLength(10)
            .IsUnicode(false)
            .HasDefaultValue("PENDING")
            .IsRequired();

        builder.Property(x => x.ManagerNote)
            .HasColumnName("manager_note")
            .HasMaxLength(1000);

        builder.Property(x => x.ApprovedByUserId)
            .HasColumnName("approved_by_user_id");

        builder.Property(x => x.ApprovedAt)
            .HasColumnName("approved_at")
            .HasColumnType("datetime2");

        builder.Property(x => x.SubmittedAt)
            .HasColumnName("submitted_at")
            .HasColumnType("datetime2");

        builder.Property(x => x.EvaluatedAt)
            .HasColumnName("evaluated_at")
            .HasColumnType("datetime2");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(x => x.SubmissionNo)
            .IsUnique();

        builder.HasIndex(x => new { x.TemplateVersionId, x.ReportDate, x.Status });

        builder.HasIndex(x => new { x.CreatedByUserId, x.CreatedAt })
            .IsDescending(false, true);

        builder.HasOne(x => x.TemplateVersion)
            .WithMany(x => x.Submissions)
            .HasForeignKey(x => x.TemplateVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedSubmissions)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ApprovedByUser)
            .WithMany(x => x.ApprovedSubmissions)
            .HasForeignKey(x => x.ApprovedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
