using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzunuInkisaf.Application.Services;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Auth;
using OzunuInkisaf.Contracts.Users;

namespace OzunuInkisaf.WebApi.Controllers;

/// <summary>
/// Admin tərəfindən istifadəçi idarəetməsi: yaratma (tək/toplu), aktivlik
/// vəziyyəti və şifrə sıfırlama. Hamısı "AdminOnly" siyasəti ilə qorunur.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IUserService userService, IAuthService authService, ICurrentUserService currentUser)
    {
        _userService = userService;
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>Bütün istifadəçilərin siyahısı, opsional axtarışla.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserSummaryDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await _userService.GetAllAsync(search, cancellationToken);
        return Ok(result);
    }

    /// <summary>Tək istifadəçi yaradır; ad/şifrə göstərilməzsə avtomatik generasiya olunur.</summary>
    [HttpPost]
    public async Task<ActionResult<CreateUserResult>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.CreateAsync(request, _currentUser.UserId!.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>Göstərilən sayda istifadəçini avtomatik ad/şifrə ilə toplu yaradır.</summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<BulkCreateUsersResult>> BulkCreate([FromBody] BulkCreateUsersRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.BulkCreateAsync(request.Count, _currentUser.UserId!.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>Ad/soyad siyahısından (CSV-dən idxal və s.) toplu istifadəçi yaradır.</summary>
    [HttpPost("bulk-from-list")]
    public async Task<ActionResult<BulkCreateUsersResult>> BulkCreateFromList([FromBody] BulkCreateUsersRequestFromList request, CancellationToken cancellationToken)
    {
        var result = await _userService.BulkCreateFromListAsync(request, _currentUser.UserId!.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>İstifadəçini aktivləşdirir/deaktiv edir (silinmir, sadəcə girişi bağlanır).</summary>
    [HttpPost("{userId:guid}/active")]
    public async Task<IActionResult> SetActive(Guid userId, [FromQuery] bool isActive, CancellationToken cancellationToken)
    {
        await _userService.SetActiveAsync(userId, isActive, cancellationToken);
        return NoContent();
    }

    /// <summary>Yeni təsadüfi şifrə generasiya edir və istifadəçiyə təyin edir.</summary>
    [HttpPost("{userId:guid}/reset-password")]
    public async Task<ActionResult<CreateUserResult>> ResetPassword(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _userService.ResetPasswordAsync(userId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// İstifadəçini və ona aid şəxsi qeydləri tamamilə silir (admin silinə bilməz).
    /// </summary>
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Admin-in bu istifadəçinin hesabına şifrəsini bilmədən birbaşa daxil
    /// olması üçün onun adına bir token verir (məs. lazım olduqda şifrəsini
    /// dəyişmək üçün). Client tərəf admin-in öz token-ini müvəqqəti saxlayıb
    /// bu token-lə əvəzləyir, sonra "Adminə qayıt" ilə geri qayıda bilir.
    /// </summary>
    [HttpPost("{userId:guid}/impersonate")]
    public async Task<ActionResult<LoginResponse>> Impersonate(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _authService.ImpersonateAsync(userId, cancellationToken);
        return Ok(result);
    }
}
