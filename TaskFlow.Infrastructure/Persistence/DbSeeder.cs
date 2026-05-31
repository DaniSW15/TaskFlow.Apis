using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Persistence;

public static class DbSeeder
{
    /// <summary>
    /// Seeds one user per role if none of the seed accounts exist yet.
    /// Safe to run on every startup — only inserts if missing.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db, IPasswordService passwordService, ILogger logger)
    {
        var seedEmails = new[]
        {
            "admin@taskflow.dev",
            "analyst@taskflow.dev",
            "member@taskflow.dev",
            "client@taskflow.dev",
        };

        // Skip if all seed users already exist
        var existingCount = await db.Users
            .IgnoreQueryFilters()
            .CountAsync(u => seedEmails.Contains(u.Email));

        if (existingCount >= seedEmails.Length)
        {
            logger.LogInformation("DbSeeder: seed users already present, skipping.");
            return;
        }

        var now = DateTime.UtcNow;

        var users = new List<User>
        {
            new()
            {
                Id         = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                FirstName  = "Admin",
                LastName   = "TaskFlow",
                Email      = "admin@taskflow.dev",
                PasswordHash = passwordService.Hash("Admin1234!"),
                Role       = UserRole.Admin,
                CreatedAt  = now,
                UpdatedAt  = now,
            },
            new()
            {
                Id         = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                FirstName  = "Ana",
                LastName   = "Analyst",
                Email      = "analyst@taskflow.dev",
                PasswordHash = passwordService.Hash("Analyst1234!"),
                Role       = UserRole.Analyst,
                CreatedAt  = now,
                UpdatedAt  = now,
            },
            new()
            {
                Id         = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                FirstName  = "Marco",
                LastName   = "Member",
                Email      = "member@taskflow.dev",
                PasswordHash = passwordService.Hash("Member1234!"),
                Role       = UserRole.Member,
                CreatedAt  = now,
                UpdatedAt  = now,
            },
            new()
            {
                Id         = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                FirstName  = "Carlos",
                LastName   = "Client",
                Email      = "client@taskflow.dev",
                PasswordHash = passwordService.Hash("Client1234!"),
                Role       = UserRole.Client,
                CreatedAt  = now,
                UpdatedAt  = now,
            },
        };

        // Only insert users that don't already exist
        foreach (var user in users)
        {
            var exists = await db.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == user.Email);

            if (!exists)
                await db.Users.AddAsync(user);
        }

        await db.SaveChangesAsync();

        logger.LogInformation(
            "DbSeeder: seed users created — admin@taskflow.dev / analyst@taskflow.dev / member@taskflow.dev / client@taskflow.dev");
    }
}
