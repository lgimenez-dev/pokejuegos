using PokeJuegos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PokeJuegos.Infrastructure.Configurations;

internal sealed class UserConfigConfiguration : IEntityTypeConfiguration<UserConfig>
{
    public void Configure(EntityTypeBuilder<UserConfig> builder)
    {
        builder.ToTable("user_configs");

        builder.HasKey(c => c.UserId)
               .HasName("PK_user_configs");

        builder.Property(c => c.UserId)
               .HasColumnName("user_id")
               .HasColumnType("uuid")
               .ValueGeneratedNever();

        builder.Property(c => c.Language)
               .HasColumnName("language")
               .HasColumnType("varchar(2)")
               .IsRequired(false);

        builder.Property(c => c.Sound)
               .HasColumnName("sound")
               .HasColumnType("boolean")
               .IsRequired();

        builder.Property(c => c.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamptz")
               .IsRequired()
               .HasDefaultValueSql("now()");

        builder.Property(c => c.UpdatedAt)
               .HasColumnName("updated_at")
               .HasColumnType("timestamptz")
               .IsRequired();

        builder.HasOne<User>()
               .WithOne()
               .HasForeignKey<UserConfig>(c => c.UserId)
               .HasConstraintName("FK_user_configs_user")
               .OnDelete(DeleteBehavior.Cascade);
    }
}
