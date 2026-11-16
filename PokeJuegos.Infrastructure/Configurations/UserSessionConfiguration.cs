using PokeJuegos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PokeJuegos.Infrastructure.Configurations;

internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(s => s.Id)
               .HasName("PK_user_sessions");

        builder.Property(s => s.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(s => s.UserId)
               .HasColumnName("user_id")
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(s => s.TokenHash)
               .HasColumnName("token_hash")
               .HasColumnType("varchar(255)")
               .IsRequired();

        builder.Property(s => s.UserAgent)
               .HasColumnName("user_agent")
               .HasColumnType("varchar(512)")
               .IsRequired(false);

        builder.Property(s => s.Ip)
               .HasColumnName("ip")
               .HasColumnType("varchar(64)")
               .IsRequired(false);

        builder.Property(s => s.ExpirationDate)
               .HasColumnName("expiration_date")
               .HasColumnType("timestamptz")
               .IsRequired();

        builder.Property(s => s.RevokedAt)
               .HasColumnName("revoked_at")
               .HasColumnType("timestamptz")
               .IsRequired(false);

        builder.Property(s => s.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamptz")
               .IsRequired()
               .HasDefaultValueSql("now()");

        builder.Property(s => s.UpdatedAt)
               .HasColumnName("updated_at")
               .HasColumnType("timestamptz")
               .IsRequired(false);

        builder.Ignore(s => s.IsValid);

        builder.HasOne<User>()
               .WithMany()
               .HasForeignKey(s => s.UserId)
               .HasConstraintName("FK_user_sessions_user")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.UserId)
               .HasDatabaseName("IDX_user_sessions_user_id");

        builder.HasIndex(s => s.TokenHash)
               .HasDatabaseName("IDX_user_sessions_token_hash");

        builder.HasIndex(s => s.CreatedAt)
               .HasDatabaseName("IDX_user_sessions_created_at");
    }
}
