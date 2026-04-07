using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Configurations;

public class ReportTemplateVersionConfiguration : IEntityTypeConfiguration<ReportTemplateVersion>
{
    public void Configure(EntityTypeBuilder<ReportTemplateVersion> builder)
    {
        builder.ToTable("report_template_versions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TemplateId)
            .HasColumnName("template_id")
            .IsRequired();

        builder.Property(x => x.VersionNo)
            .HasColumnName("version_no")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .HasColumnName("effective_from")
            .HasColumnType("datetime2");

        builder.Property(x => x.EffectiveTo)
            .HasColumnName("effective_to")
            .HasColumnType("datetime2");

        builder.Property(x => x.PublishedBy)
            .HasColumnName("published_by");

        builder.Property(x => x.PublishedAt)
            .HasColumnName("published_at")
            .HasColumnType("datetime2");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasIndex(x => new { x.TemplateId, x.VersionNo })
            .IsUnique();

        builder.HasOne(x => x.Template)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PublishedByUser)
            .WithMany(x => x.PublishedTemplateVersions)
            .HasForeignKey(x => x.PublishedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
