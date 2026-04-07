using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Configurations;

public class ReportAttachmentConfiguration : IEntityTypeConfiguration<ReportAttachment>
{
    public void Configure(EntityTypeBuilder<ReportAttachment> builder)
    {
        builder.ToTable("report_attachments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SubmissionId)
            .HasColumnName("submission_id")
            .IsRequired();

        builder.Property(x => x.FilePath)
            .HasColumnName("file_path")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(100)
            .IsUnicode(false);

        builder.Property(x => x.FileSizeBytes)
            .HasColumnName("file_size_bytes");

        builder.Property(x => x.CapturedAt)
            .HasColumnName("captured_at")
            .HasColumnType("datetime2");

        builder.Property(x => x.UploadedByUserId)
            .HasColumnName("uploaded_by_user_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(x => new { x.SubmissionId, x.CreatedAt })
            .IsDescending(false, true);

        builder.HasOne(x => x.Submission)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UploadedByUser)
            .WithMany(x => x.UploadedAttachments)
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
