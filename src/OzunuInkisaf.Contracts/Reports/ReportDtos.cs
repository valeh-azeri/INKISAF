using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.Contracts.Reports;

public enum ReportPeriodDto
{
    Weekly = 0,
    Monthly = 1,
    Yearly = 2,
    Custom = 3
}

public record ReportFilterRequest(
    ReportPeriodDto Period,
    DateOnly? From,
    DateOnly? To,
    bool IncludeBookReading,
    bool IncludeKhatim,
    bool IncludeTally);

public record LeaderboardRowDto(
    int Rank,
    Guid UserId,
    string FullName,
    int PagesRead,
    int JuzParticipation,
    int TallyPoints,
    int TotalScore);

public record LeaderboardResultDto(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<LeaderboardRowDto> Rows);

public record AdminDashboardSummaryDto(
    int ActiveUserCount,
    int ActiveUserCountDeltaThisMonth,
    int TotalBooksCount,
    int BooksAddedThisMonth,
    int KhatimCompletedJuz,
    int KhatimTotalJuz,
    int AverageWeeklyTallyScore,
    IReadOnlyList<LeaderboardRowDto> TopFiveThisWeek);

public record DeclareWinnerRequest(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    bool IncludeBookReading,
    bool IncludeKhatim,
    bool IncludeTally);

public record WinnerAwardDto(
    Guid Id,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    Guid UserId,
    string UserFullName,
    int TotalScore,
    DateTime AnnouncedAt);
