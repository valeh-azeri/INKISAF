using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Auth;
using OzunuInkisaf.Application.Services;

namespace OzunuInkisaf.WebApi.Controllers;

/// <summary>
/// Giriş və şifrə əməliyyatları. Bütün istifadəçilər (admin daxil) eyni
/// endpoint-dən giriş edir — rol JWT token-in içində gəlir.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>İstifadəçi adı və şifrə ilə giriş edir, JWT token qaytarır.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Giriş etmiş istifadəçi öz şifrəsini dəyişir.</summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        await _authService.ChangePasswordAsync(userId, request, cancellationToken);
        return NoContent();
    }
}
