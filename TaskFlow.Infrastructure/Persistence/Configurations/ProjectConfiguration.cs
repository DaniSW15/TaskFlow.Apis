using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ClientId);
        builder.HasIndex(p => p.AnalystId);
        builder.HasIndex(p => p.IsDeleted);

        // Un proyecto pertenece a un cliente
        builder.HasOne(p => p.Client)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un analista (User) gestiona el proyecto
        builder.HasOne(p => p.Analyst)
            .WithMany(u => u.ManagedProjects)
            .HasForeignKey(p => p.AnalystId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un proyecto contiene varios boards
        builder.HasMany(p => p.Boards)
            .WithOne(b => b.Project)
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.SetNull); // si se elimina proyecto, boards quedan sin proyecto
    }
}
