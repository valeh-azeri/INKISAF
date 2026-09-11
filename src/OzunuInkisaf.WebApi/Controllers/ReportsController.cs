using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzunuInkisaf.Application.Services;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Reports;

namespace OzunuInkisaf.WebApi.Controllers;

/// <summary>
/// Admin hesabatları: gündəlik xülasə (dashboard), seçilmiş dövr üzrə
/// liderlik cədvəli (həftə/ay/il/xüsusi aralıq) və qalib elanı.
/// </summary>
[ApiController]
[Route("api/reports")]
[Authorize(Policy = "AdminOnly")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ICurrentUserService _currentUser;

    public ReportsController(IReportService reportService, ICurrentUserService currentUser)
    {
        _reportService = reportService;
        _currentUser = currentUser;
    }

    /// <summary>Admin ana səhifəsi üçün ümumi xülasə göstəriciləri.</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardSummaryDto>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var result = await _reportService.GetDashboardSummaryAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Seçilmiş dövr və kateqoriyalara görə liderlik cədvəli.</summary>
    [HttpPost("leaderboard")]
    public async Task<ActionResult<LeaderboardResultDto>> GetLeaderboard([FromBody] ReportFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetLeaderboardAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>Seçilmiş dövr və kateqoriyalara əsasən qalibi elan edir.</summary>
    [HttpPost("winners")]
    public async Task<ActionResult<WinnerAwardDto>> DeclareWinner([FromBody] DeclareWinnerRequest request, CancellationToken cancellationToken)
    {
        var result = await _reportService.DeclareWinnerAsync(request, _currentUser.UserId!.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>Bütün zamanlarda elan olunmuş qaliblərin siyahısı.</summary>
    [HttpGet("winners")]
    public async Task<ActionResult<IReadOnlyList<WinnerAwardDto>>> GetWinners(CancellationToken cancellationToken)
    {
        var result = await _reportService.GetWinnersAsync(cancellationToken);
        return Ok(result);
    }
}
