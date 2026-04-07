using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Configurations;

public class ApprovalLogConfiguration : IEntityTypeConfiguration<ApprovalLog>
{
    public void Configure(EntityTypeBuilder<ApprovalLog> builder)
    {
        builder.ToTable("approval_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SubmissionId)
            .HasColumnName("submission_id")
            .IsRequired();

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.FromStatus)
            .HasColumnName("from_status")
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(x => x.ToStatus)
            .HasColumnName("to_status")
            .HasMaxLength(30)
            .IsUnicode(false);

        builder.Property(x => x.ActionByUserId)
            .HasColumnName("action_by_user_id");

        builder.Property(x => x.Comment)
            .HasColumnName("comment")
            .HasMaxLength(1000);

        builder.Property(x => x.MetadataJson)
            .HasColumnName("metadata_json")
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ActionAt)
            .HasColumnName("action_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(x => new { x.SubmissionId, x.ActionAt })
            .IsDescending(false, true);

        builder.HasOne(x => x.Submission)
            .WithMany(x => x.ApprovalLogs)
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ActionByUser)
            .WithMany(x => x.ApprovalLogs)
            .HasForeignKey(x => x.ActionByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
