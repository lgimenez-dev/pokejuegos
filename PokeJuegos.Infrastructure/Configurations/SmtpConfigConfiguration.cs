using PokeJuegos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PokeJuegos.Infrastructure.Configurations;

internal sealed class SmtpConfigConfiguration : IEntityTypeConfiguration<SmtpConfig>
{
    public void Configure(EntityTypeBuilder<SmtpConfig> builder)
    {
        builder.ToTable("smtp_configs");

        builder.HasKey(c => c.Id)
               .HasName("PK_smtp_configs");

        builder.Property(c => c.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(c => c.Host)
               .HasColumnName("host")
               .HasColumnType("varchar(256)")
               .IsRequired();

        builder.Property(c => c.Port)
               .HasColumnName("port")
               .HasColumnType("integer")
               .IsRequired()
               .HasDefaultValue(587);

        builder.Property(c => c.Email)
               .HasColumnName("email")
               .HasColumnType("varchar(256)")
               .IsRequired();

        builder.Property(c => c.Password)
               .HasColumnName("password")
               .HasColumnType("varchar(256)")
               .IsRequired();

        builder.Property(c => c.SenderName)
               .HasColumnName("sender_name")
               .HasColumnType("varchar(200)")
               .IsRequired();

        builder.Property(c => c.UseTls)
               .HasColumnName("use_tls")
               .HasColumnType("boolean")
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamptz")
               .IsRequired()
               .HasDefaultValueSql("now()");

        builder.Property(c => c.UpdatedAt)
               .HasColumnName("updated_at")
               .HasColumnType("timestamptz")
               .IsRequired(false);
    }
}
