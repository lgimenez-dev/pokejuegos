using PokeJuegos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PokeJuegos.Infrastructure.Configurations;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");

        builder.HasKey(p => p.Id)
               .HasName("PK_persons");

        builder.Property(p => p.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .HasDefaultValueSql("uuid_generate_v4()");

        builder.Property(p => p.UserId)
               .HasColumnName("user_id")
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(p => p.Name)
               .HasColumnName("name")
               .HasColumnType("varchar(200)")
               .IsRequired();

        builder.Property(p => p.NationalId)
               .HasColumnName("national_id")
               .HasColumnType("varchar(20)")
               .IsRequired(false);

        builder.Property(p => p.Phone)
               .HasColumnName("phone")
               .HasColumnType("varchar(20)")
               .IsRequired(false);

        builder.Property(p => p.BirthDate)
               .HasColumnName("birth_date")
               .HasColumnType("date")
               .IsRequired(false);

        builder.Property(p => p.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamptz")
               .IsRequired()
               .HasDefaultValueSql("now()");

        builder.Property(p => p.UpdatedAt)
               .HasColumnName("updated_at")
               .HasColumnType("timestamptz")
               .IsRequired(false);

        builder.HasOne<User>()
               .WithOne()
               .HasForeignKey<Person>(p => p.UserId)
               .HasConstraintName("FK_persons_user")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId)
               .HasDatabaseName("UQ_persons_user_id")
               .IsUnique();

        builder.HasIndex(p => p.NationalId)
               .HasDatabaseName("UQ_persons_national_id")
               .IsUnique()
               .HasFilter("national_id IS NOT NULL");
    }
}
