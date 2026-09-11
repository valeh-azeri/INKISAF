using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Domain.Entities;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Infrastructure.Persistence.Seed;

/// <summary>
/// Runs once at startup (see Program.cs). Applies any pending EF Core
/// migrations and guarantees exactly one Admin account exists with the
/// username/password the project owner asked for: admin / admin.
///
/// IMPORTANT: this is a deliberately weak, well-known password meant only
/// to get the app running for the first time. MustChangePassword is set to
/// false here on purpose so the seeded login works immediately, but you
/// should change this password (or the seed values below) before using
/// this anywhere the app is reachable by more than your own team.
/// </summary>
public static class DbInitializer
{
    public const string SeedAdminUsername = "admin";
    public const string SeedAdminPassword = "admin";

    public static async Task InitializeAsync(ApplicationDbContext db, IPasswordHasher passwordHasher, ILogger logger)
    {
        await db.Database.MigrateAsync();

        var adminExists = await db.Users.AnyAsync(u => u.Role == UserRole.Admin);
        if (!adminExists)
        {
            var (hash, salt) = passwordHasher.HashPassword(SeedAdminPassword);

            db.Users.Add(new User
            {
                FullName = "Administrator",
                Username = SeedAdminUsername,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = UserRole.Admin,
                IsActive = true,
                MustChangePassword = false,
            });

            await db.SaveChangesAsync();

            logger.LogWarning(
                "Seeded the first admin account — username '{Username}', password '{Password}'. Change this password immediately after first login in a real deployment.",
                SeedAdminUsername, SeedAdminPassword);
        }
    }
}
