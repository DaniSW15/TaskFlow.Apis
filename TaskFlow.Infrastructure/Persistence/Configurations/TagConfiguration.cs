using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Color)
            .IsRequired()
            .HasMaxLength(7); // formato hex #RRGGBB

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.HasIndex(t => t.IsDeleted);

        // Relación N:M con TaskItem — EF crea tabla TaskItemTags automáticamente
        builder.HasMany(t => t.Tasks)
            .WithMany(ti => ti.Tags)
            .UsingEntity(j => j.ToTable("TaskItemTags"));
    }
}
