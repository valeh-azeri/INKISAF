using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzunuInkisaf.Application.Services;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Khatim;

namespace OzunuInkisaf.WebApi.Controllers;

/// <summary>
/// Xətim (Quran tamamlama) dövrəsi: 30 cüzün götürülməsi, tamamlanması və
/// dövrələr tarixçəsi. Bütün 30 cüz tamamlananda avtomatik yeni dövrə açılır.
/// </summary>
[ApiController]
[Route("api/khatim")]
[Authorize]
public class KhatimController : ControllerBase
{
    private readonly IKhatimService _khatimService;
    private readonly ICurrentUserService _currentUser;

    public KhatimController(IKhatimService khatimService, ICurrentUserService currentUser)
    {
        _khatimService = khatimService;
        _currentUser = currentUser;
    }

    /// <summary>Cari (aktiv) xətim dövrəsinin vəziyyəti — 30 cüzün kim tərəfindən götürüldüyü.</summary>
    [HttpGet("status")]
    public async Task<ActionResult<KhatimCycleStatusDto2>> GetCurrentStatus(CancellationToken cancellationToken)
    {
        var result = await _khatimService.GetCurrentStatusAsync(_currentUser.UserId!.Value, _currentUser.IsAdmin, cancellationToken);
        return Ok(result);
    }

    /// <summary>İstifadəçi özü üçün boş bir cüzü götürür.</summary>
    [HttpPost("claim")]
    public async Task<ActionResult<KhatimCycleStatusDto2>> ClaimJuz([FromBody] ClaimJuzRequest request, CancellationToken cancellationToken)
    {
        var result = await _khatimService.ClaimJuzAsync(_currentUser.UserId!.Value, request, _currentUser.IsAdmin, cancellationToken);
        return Ok(result);
    }

    /// <summary>Götürülmüş cüzü tamamlanmış kimi qeyd edir; bütün cüzlər bitərsə yeni dövrə açılır.</summary>
    [HttpPost("complete")]
    public async Task<ActionResult<KhatimCycleStatusDto2>> CompleteJuz([FromBody] CompleteJuzRequest request, CancellationToken cancellationToken)
    {
        var result = await _khatimService.CompleteJuzAsync(_currentUser.UserId!.Value, request, _currentUser.IsAdmin, cancellationToken);
        return Ok(result);
    }

    /// <summary>Bitmiş/davam edən bütün xətim dövrələrinin tarixçəsi (admin üçün).</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<KhatimHistoryItemDto>>> GetHistory(CancellationToken cancellationToken)
    {
        var result = await _khatimService.GetHistoryAsync(cancellationToken);
        return Ok(result);
    }
}
