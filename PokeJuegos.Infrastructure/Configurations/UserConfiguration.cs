using PokeJuegos.Common.Enums;
using PokeJuegos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PokeJuegos.Infrastructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id)
               .HasName("PK_users");

        builder.Property(u => u.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(u => u.Email)
               .HasColumnName("email")
               .HasColumnType("varchar(256)")
               .IsRequired();

        builder.Property(u => u.Username)
               .HasColumnName("username")
               .HasColumnType("varchar(32)")
               .IsRequired(false);

        builder.Property(u => u.PasswordHash)
               .HasColumnName("password")
               .HasColumnType("text")
               .IsRequired(false);

        builder.Property(u => u.Role)
               .HasColumnName("role")
               .IsRequired()
               .HasUpperCaseConversion(20)
               .HasDefaultValue(UserRole.Member);

        builder.Property(u => u.Status)
               .HasColumnName("status")
               .IsRequired()
               .HasUpperCaseConversion(20)
               .HasDefaultValue(UserStatus.Active);

        builder.Property(u => u.RefreshToken)
               .HasColumnName("refresh_token")
               .HasColumnType("varchar(512)")
               .IsRequired(false);

        builder.Property(u => u.ExpirationDate)
               .HasColumnName("expiration_date")
               .HasColumnType("timestamptz")
               .IsRequired(false);

        builder.Property(u => u.Provider)
               .HasColumnName("provider")
               .IsRequired()
               .HasUpperCaseConversion(20)
               .HasDefaultValue(UserProvider.Local);

        builder.Property(u => u.ProviderId)
               .HasColumnName("provider_id")
               .HasColumnType("varchar(255)")
               .IsRequired(false);

        builder.Property(u => u.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamptz")
               .IsRequired()
               .HasDefaultValueSql("now()");

        builder.Property(u => u.UpdatedAt)
               .HasColumnName("updated_at")
               .HasColumnType("timestamptz")
               .IsRequired(false);

        builder.HasIndex(u => u.Email)
               .HasDatabaseName("UQ_users_email")
               .IsUnique();

        builder.HasIndex(u => u.Username)
               .HasDatabaseName("UQ_users_username")
               .IsUnique()
               .HasFilter("username IS NOT NULL");

        builder.HasIndex(u => u.RefreshToken)
               .HasDatabaseName("IX_users_refresh_token")
               .HasFilter("refresh_token IS NOT NULL");

        builder.HasIndex(u => new { u.Provider, u.ProviderId })
               .HasDatabaseName("IDX_users_provider_id")
               .HasFilter("provider_id IS NOT NULL");
    }
}
