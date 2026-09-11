using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzunuInkisaf.Application.Services;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Duas;

namespace OzunuInkisaf.WebApi.Controllers;

/// <summary>Dualar bölməsi: istənilən vaxt oxumaq üçün admin tərəfindən yerləşdirilən mətnlər.</summary>
[ApiController]
[Route("api/duas")]
[Authorize]
public class DuasController : ControllerBase
{
    private readonly IDuaService _duaService;
    private readonly ICurrentUserService _currentUser;

    public DuasController(IDuaService duaService, ICurrentUserService currentUser)
    {
        _duaService = duaService;
        _currentUser = currentUser;
    }

    /// <summary>Bütün aktiv duaların siyahısı, opsional kateqoriya/axtarış filtri ilə.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DuaDto>>> GetAll([FromQuery] string? category, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await _duaService.GetAllAsync(category, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>Yeni dua əlavə edir.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<DuaDto>> Create([FromBody] UpsertDuaRequest request, CancellationToken cancellationToken)
    {
        var result = await _duaService.CreateAsync(request, _currentUser.UserId!.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>Mövcud duanı yeniləyir.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{duaId:guid}")]
    public async Task<ActionResult<DuaDto>> Update(Guid duaId, [FromBody] UpsertDuaRequest request, CancellationToken cancellationToken)
    {
        var result = await _duaService.UpdateAsync(duaId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Duanı silir.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{duaId:guid}")]
    public async Task<IActionResult> Delete(Guid duaId, CancellationToken cancellationToken)
    {
        await _duaService.DeleteAsync(duaId, cancellationToken);
        return NoContent();
    }
}
