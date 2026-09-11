using Microsoft.EntityFrameworkCore;
using OzunuInkisaf.Domain.Entities;

namespace OzunuInkisaf.Application.Common.Interfaces;

/// <summary>
/// The persistence contract the Application layer codes against. Infrastructure's
/// ApplicationDbContext implements this so Application never takes a direct
/// dependency on EF Core's SqlServer provider or on Infrastructure itself.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<Book> Books { get; }

    DbSet<ReadingProgress> ReadingProgresses { get; }

    DbSet<Dua> Duas { get; }

    DbSet<KhatimCycle> KhatimCycles { get; }

    DbSet<JuzClaim> JuzClaims { get; }

    DbSet<TallyTemplate> TallyTemplates { get; }

    DbSet<TallyItem> TallyItems { get; }

    DbSet<TallyEntry> TallyEntries { get; }

    DbSet<PointsTransaction> PointsTransactions { get; }

    DbSet<WinnerAward> WinnerAwards { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
