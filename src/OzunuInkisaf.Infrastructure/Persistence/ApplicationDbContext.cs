using Microsoft.EntityFrameworkCore;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Domain.Entities;

namespace OzunuInkisaf.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<ReadingProgress> ReadingProgresses => Set<ReadingProgress>();

    public DbSet<Dua> Duas => Set<Dua>();

    public DbSet<KhatimCycle> KhatimCycles => Set<KhatimCycle>();

    public DbSet<JuzClaim> JuzClaims => Set<JuzClaim>();

    public DbSet<TallyTemplate> TallyTemplates => Set<TallyTemplate>();

    public DbSet<TallyItem> TallyItems => Set<TallyItem>();

    public DbSet<TallyEntry> TallyEntries => Set<TallyEntry>();

    public DbSet<PointsTransaction> PointsTransactions => Set<PointsTransaction>();

    public DbSet<WinnerAward> WinnerAwards => Set<WinnerAward>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
