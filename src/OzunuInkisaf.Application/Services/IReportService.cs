using OzunuInkisaf.Contracts.Reports;

namespace OzunuInkisaf.Application.Services;

public interface IReportService
{
    Task<AdminDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);

    Task<LeaderboardResultDto> GetLeaderboardAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default);

    Task<WinnerAwardDto> DeclareWinnerAsync(DeclareWinnerRequest request, Guid announcedByUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WinnerAwardDto>> GetWinnersAsync(CancellationToken cancellationToken = default);
}
