using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReportSystem.Domain.Entities;

namespace ReportSystem.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.EmployeeCode)
            .HasColumnName("employee_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(200);

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

        builder.HasIndex(x => x.EmployeeCode)
            .IsUnique();

        builder.HasIndex(x => x.Email)
            .HasFilter("[email] IS NOT NULL")
            .IsUnique();
    }
}
