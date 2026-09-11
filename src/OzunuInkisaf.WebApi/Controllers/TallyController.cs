using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzunuInkisaf.Application.Services;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Tally;

namespace OzunuInkisaf.WebApi.Controllers;

/// <summary>
/// Həftəlik çətələ: admin şablon yaradıb yayımlayır, hər istifadəçi öz
/// vəziyyətini gün-gün işarələyir, admin bütün şablonları və doldurma
/// nisbətlərini görür.
/// </summary>
[ApiController]
[Route("api/tally")]
[Authorize]
public class TallyController : ControllerBase
{
    private readonly ITallyService _tallyService;
    private readonly ICurrentUserService _currentUser;

    public TallyController(ITallyService tallyService, ICurrentUserService currentUser)
    {
        _tallyService = tallyService;
        _currentUser = currentUser;
    }

    /// <summary>Yeni çətələ şablonu yaradır (hələ "draft" statusunda, istifadəçilərə görünmür).</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<TallyTemplateDto>> Create([FromBody] CreateTallyTemplateRequest request, CancellationToken cancellationToken)
    {
        var result = await _tallyService.CreateAsync(request, _currentUser.UserId!.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>Şablonu yayımlayır — bundan sonra bütün istifadəçilər görüb işarələyə bilər.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{templateId:guid}/publish")]
    public async Task<ActionResult<TallyTemplateDto>> Publish(Guid templateId, CancellationToken cancellationToken)
    {
        var result = await _tallyService.PublishAsync(templateId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Cari istifadəçi üçün aktiv (yayımlanmış, hələ bağlanmamış) çətələ.</summary>
    [HttpGet("active")]
    public async Task<ActionResult<TallyTemplateDto?>> GetActive(CancellationToken cancellationToken)
    {
        var result = await _tallyService.GetActiveForUserAsync(_currentUser.UserId!.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>Bir günün bir maddəsini yerinə yetirilmiş/yetirilməmiş kimi işarələyir.</summary>
    [HttpPut("entry")]
    public async Task<ActionResult<TallyTemplateDto>> SetEntry([FromBody] SetTallyEntryRequest request, CancellationToken cancellationToken)
    {
        var result = await _tallyService.SetEntryAsync(_currentUser.UserId!.Value, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Bütün çətələ şablonlarının qısa siyahısı (admin üçün).</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("summaries")]
    public async Task<ActionResult<IReadOnlyList<TallyTemplateSummaryDto>>> GetSummaries(CancellationToken cancellationToken)
    {
        var result = await _tallyService.GetSummariesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Bir şablonun tam təfərrüatı (admin üçün).</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("{templateId:guid}")]
    public async Task<ActionResult<TallyTemplateDto>> GetById(Guid templateId, CancellationToken cancellationToken)
    {
        var result = await _tallyService.GetByIdForAdminAsync(templateId, cancellationToken);
        return Ok(result);
    }
}
