using Microsoft.EntityFrameworkCore;
using OzunuInkisaf.Application.Common.Exceptions;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Reports;
using OzunuInkisaf.Domain.Entities;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Application.Services;

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _db;

    public ReportService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AdminDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var activeUsers = await _db.Users.CountAsync(u => u.Role == UserRole.User && u.IsActive, cancellationToken);
        var newUsersThisMonth = await _db.Users.CountAsync(u => u.Role == UserRole.User && u.CreatedAt >= monthStart, cancellationToken);

        var totalBooks = await _db.Books.CountAsync(b => b.Status == BookStatus.Active, cancellationToken);
        var booksThisMonth = await _db.Books.CountAsync(b => b.Status == BookStatus.Active && b.CreatedAt >= monthStart, cancellationToken);

        var currentCycle = await _db.KhatimCycles
            .Include(c => c.JuzClaims)
            .Where(c => c.Status == KhatimCycleStatus.InProgress)
            .OrderByDescending(c => c.CycleNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var khatimCompleted = currentCycle?.JuzClaims.Count(j => j.IsCompleted) ?? 0;

        var weekAgo = DateOnly.FromDateTime(now).AddDays(-7);
        var weeklyTallyPointsAvg = await _db.PointsTransactions
            .Where(p => p.Source == PointsSource.TallyEntry && p.EarnedAt >= weekAgo.ToDateTime(TimeOnly.MinValue))
            .GroupBy(p => p.UserId)
            .Select(g => g.Sum(x => x.Points))
            .ToListAsync(cancellationToken);

        var avgWeekly = weeklyTallyPointsAvg.Count > 0 ? (int)Math.Round(weeklyTallyPointsAvg.Average()) : 0;

        var topFive = await GetLeaderboardAsync(
            new ReportFilterRequest(ReportPeriodDto.Weekly, weekAgo, DateOnly.FromDateTime(now), true, true, true),
            cancellationToken);

        return new AdminDashboardSummaryDto(
            activeUsers,
            newUsersThisMonth,
            totalBooks,
            booksThisMonth,
            khatimCompleted,
            KhatimService.TotalJuz,
            avgWeekly,
            topFive.Rows.Take(5).ToList());
    }

    public async Task<LeaderboardResultDto> GetLeaderboardAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var (from, to) = ResolvePeriod(filter);
        var fromUtc = from.ToDateTime(TimeOnly.MinValue);
        var toUtc = to.ToDateTime(TimeOnly.MaxValue);

        var users = await _db.Users.Where(u => u.Role == UserRole.User).ToListAsync(cancellationToken);
        var userIds = users.Select(u => u.Id).ToList();

        var pagesRead = filter.IncludeBookReading
            ? await _db.ReadingProgresses
                .Where(p => userIds.Contains(p.UserId) && p.LastReadAt >= fromUtc && p.LastReadAt <= toUtc)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Pages = g.Sum(x => x.CurrentPage) })
                .ToDictionaryAsync(x => x.UserId, x => x.Pages, cancellationToken)
            : new Dictionary<Guid, int>();

        var juzParticipation = filter.IncludeKhatim
            ? await _db.JuzClaims
                .Where(j => j.UserId != null && userIds.Contains(j.UserId.Value) && j.ClaimedAt >= fromUtc && j.ClaimedAt <= toUtc)
                .GroupBy(j => j.UserId!.Value)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count, cancellationToken)
            : new Dictionary<Guid, int>();

        var tallyPoints = filter.IncludeTally
            ? await _db.PointsTransactions
                .Where(p => p.Source == PointsSource.TallyEntry && userIds.Contains(p.UserId) && p.EarnedAt >= fromUtc && p.EarnedAt <= toUtc)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Points = g.Sum(x => x.Points) })
                .ToDictionaryAsync(x => x.UserId, x => x.Points, cancellationToken)
            : new Dictionary<Guid, int>();

        var khatimPoints = filter.IncludeKhatim
            ? await _db.PointsTransactions
                .Where(p => p.Source == PointsSource.KhatimJuzCompleted && userIds.Contains(p.UserId) && p.EarnedAt >= fromUtc && p.EarnedAt <= toUtc)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Points = g.Sum(x => x.Points) })
                .ToDictionaryAsync(x => x.UserId, x => x.Points, cancellationToken)
            : new Dictionary<Guid, int>();

        var bookPoints = filter.IncludeBookReading
            ? await _db.PointsTransactions
                .Where(p => p.Source == PointsSource.BookCompleted && userIds.Contains(p.UserId) && p.EarnedAt >= fromUtc && p.EarnedAt <= toUtc)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Points = g.Sum(x => x.Points) })
                .ToDictionaryAsync(x => x.UserId, x => x.Points, cancellationToken)
            : new Dictionary<Guid, int>();

        var rows = users
            .Select(u =>
            {
                var pages = pagesRead.GetValueOrDefault(u.Id, 0);
                var juz = juzParticipation.GetValueOrDefault(u.Id, 0);
                var tally = tallyPoints.GetValueOrDefault(u.Id, 0);
                var total = tally + khatimPoints.GetValueOrDefault(u.Id, 0) + bookPoints.GetValueOrDefault(u.Id, 0);

                return new { u.Id, u.FullName, pages, juz, tally, total };
            })
            .Where(x => x.pages > 0 || x.juz > 0 || x.tally > 0 || x.total > 0)
            .OrderByDescending(x => x.total)
            .Select((x, index) => new LeaderboardRowDto(index + 1, x.Id, x.FullName, x.pages, x.juz, x.tally, x.total))
            .ToList();

        return new LeaderboardResultDto(from, to, rows);
    }

    public async Task<WinnerAwardDto> DeclareWinnerAsync(DeclareWinnerRequest request, Guid announcedByUserId, CancellationToken cancellationToken = default)
    {
        var leaderboard = await GetLeaderboardAsync(
            new ReportFilterRequest(ReportPeriodDto.Custom, request.PeriodStart, request.PeriodEnd, request.IncludeBookReading, request.IncludeKhatim, request.IncludeTally),
            cancellationToken);

        var winnerRow = leaderboard.Rows.FirstOrDefault()
            ?? throw new ValidationAppException("Seçilmiş dövr üçün heç bir nəticə tapılmadı.");

        var categories = new List<int>();
        if (request.IncludeBookReading) categories.Add((int)PointsSource.BookCompleted);
        if (request.IncludeKhatim) categories.Add((int)PointsSource.KhatimJuzCompleted);
        if (request.IncludeTally) categories.Add((int)PointsSource.TallyEntry);

        var award = new WinnerAward
        {
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            CategoriesCsv = categories.Count > 0 ? string.Join(',', categories) : null,
            UserId = winnerRow.UserId,
            TotalScore = winnerRow.TotalScore,
            AnnouncedByUserId = announcedByUserId,
        };

        _db.WinnerAwards.Add(award);
        await _db.SaveChangesAsync(cancellationToken);

        return new WinnerAwardDto(award.Id, award.PeriodStart, award.PeriodEnd, award.UserId, winnerRow.FullName, award.TotalScore, award.AnnouncedAt);
    }

    public async Task<IReadOnlyList<WinnerAwardDto>> GetWinnersAsync(CancellationToken cancellationToken = default)
    {
        var awards = await _db.WinnerAwards
            .Include(a => a.User)
            .OrderByDescending(a => a.AnnouncedAt)
            .ToListAsync(cancellationToken);

        return awards.Select(a => new WinnerAwardDto(
            a.Id, a.PeriodStart, a.PeriodEnd, a.UserId, a.User?.FullName ?? "?", a.TotalScore, a.AnnouncedAt)).ToList();
    }

    private static (DateOnly From, DateOnly To) ResolvePeriod(ReportFilterRequest filter)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (filter.Period == ReportPeriodDto.Custom && filter.From is not null && filter.To is not null)
        {
            return (filter.From.Value, filter.To.Value);
        }

        return filter.Period switch
        {
            ReportPeriodDto.Weekly => (today.AddDays(-7), today),
            ReportPeriodDto.Monthly => (today.AddDays(-30), today),
            ReportPeriodDto.Yearly => (today.AddYears(-1), today),
            _ => (filter.From ?? today.AddDays(-30), filter.To ?? today),
        };
    }
}
