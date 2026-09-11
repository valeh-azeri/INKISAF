using Microsoft.JSInterop;
using OzunuInkisaf.Contracts.Auth;
using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.Web.Services;

/// <summary>
/// Cari sessiyanın giriş vəziyyətini yaddaşda saxlayır və tətbiq yenidən
/// açılanda brauzerin localStorage-ı vasitəsilə bərpa edir ki, istifadəçi
/// hər dəfə yenidən giriş etmək məcburiyyətində qalmasın.
/// </summary>
public class AuthState
{
    private const string TokenKey = "oi_access_token";
    private const string ExpiryKey = "oi_token_expiry";
    private const string UserIdKey = "oi_user_id";
    private const string FullNameKey = "oi_full_name";
    private const string UsernameKey = "oi_username";
    private const string RoleKey = "oi_role";

    private readonly IJSRuntime _js;

    public AuthState(IJSRuntime js)
    {
        _js = js;
    }

    public string? AccessToken { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public Guid? UserId { get; private set; }
    public string? FullName { get; private set; }
    public string? Username { get; private set; }
    public UserRoleDto Role { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken) && ExpiresAtUtc > DateTime.UtcNow;
    public bool IsAdmin => IsAuthenticated && Role == UserRoleDto.Admin;

    /// <summary>
    /// Admin "istifadəçi kimi daxil ol" edəndə öz sessiyasını burada saxlayır
    /// ki, "Adminə qayıt" ilə geri dönə bilsin.
    /// </summary>
    private LoginResponse? _savedAdminSession;
    public bool IsImpersonating => _savedAdminSession is not null;

    public event Action? Changed;

    public void SetFromLogin(LoginResponse response)
    {
        AccessToken = response.AccessToken;
        ExpiresAtUtc = response.ExpiresAtUtc;
        UserId = response.UserId;
        FullName = response.FullName;
        Username = response.Username;
        Role = response.Role;

        Changed?.Invoke();

        _ = PersistAsync();
    }

    public void BeginImpersonation(LoginResponse targetSession)
    {
        _savedAdminSession = new LoginResponse(AccessToken!, ExpiresAtUtc!.Value, UserId!.Value, FullName!, Username!, Role, false);
        SetFromLogin(targetSession);
    }

    public void EndImpersonation()
    {
        if (_savedAdminSession is null) return;

        var adminSession = _savedAdminSession;
        _savedAdminSession = null;
        SetFromLogin(adminSession);
    }

    public void SignOut()
    {
        AccessToken = null;
        ExpiresAtUtc = null;
        UserId = null;
        FullName = null;
        Username = null;

        Changed?.Invoke();

        _ = ClearAsync();
    }

    private async Task PersistAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, AccessToken ?? string.Empty);
            await _js.InvokeVoidAsync("localStorage.setItem", ExpiryKey, ExpiresAtUtc?.ToString("O") ?? string.Empty);
            await _js.InvokeVoidAsync("localStorage.setItem", UserIdKey, UserId?.ToString() ?? string.Empty);
            await _js.InvokeVoidAsync("localStorage.setItem", FullNameKey, FullName ?? string.Empty);
            await _js.InvokeVoidAsync("localStorage.setItem", UsernameKey, Username ?? string.Empty);
            await _js.InvokeVoidAsync("localStorage.setItem", RoleKey, ((int)Role).ToString());
        }
        catch
        {
            // localStorage bəzi kontekstlərdə (məsələn private brauzinq) əlçatmaz
            // ola bilər — bu sadəcə "məni xatırla" rahatlığıdır, kritik deyil.
        }
    }

    private async Task ClearAsync()
    {
        try
        {
            foreach (var key in new[] { TokenKey, ExpiryKey, UserIdKey, FullNameKey, UsernameKey, RoleKey })
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", key);
            }
        }
        catch
        {
            // Bax yuxarı — kritik deyil.
        }
    }

    /// <summary>Tətbiq başlanğıcında əvvəlki sessiyanı bərpa etməyə çalışır.</summary>
    public async Task TryRestoreAsync()
    {
        try
        {
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            var expiryRaw = await _js.InvokeAsync<string?>("localStorage.getItem", ExpiryKey);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(expiryRaw))
            {
                return;
            }

            if (!DateTime.TryParse(expiryRaw, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiry) || expiry <= DateTime.UtcNow)
            {
                return;
            }

            var userIdRaw = await _js.InvokeAsync<string?>("localStorage.getItem", UserIdKey);
            var roleRaw = await _js.InvokeAsync<string?>("localStorage.getItem", RoleKey);

            AccessToken = token;
            ExpiresAtUtc = expiry;
            UserId = Guid.TryParse(userIdRaw, out var id) ? id : null;
            FullName = await _js.InvokeAsync<string?>("localStorage.getItem", FullNameKey);
            Username = await _js.InvokeAsync<string?>("localStorage.getItem", UsernameKey);
            Role = int.TryParse(roleRaw, out var roleValue) ? (UserRoleDto)roleValue : UserRoleDto.User;

            Changed?.Invoke();
        }
        catch
        {
            // Bərpa mümkün olmadı — istifadəçi sadəcə yenidən giriş edəcək.
        }
    }
}
