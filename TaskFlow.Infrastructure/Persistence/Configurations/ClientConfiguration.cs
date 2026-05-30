using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.Phone)
            .HasMaxLength(30);

        builder.Property(c => c.Company)
            .HasMaxLength(200);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(c => c.IsDeleted);

        builder.HasMany(c => c.Projects)
            .WithOne(p => p.Client)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict); // no borrar cliente si tiene proyectos activos
    }
}
