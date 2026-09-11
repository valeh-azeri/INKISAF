using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.Contracts.Auth;

public record LoginRequest(string Username, string Password);

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string FullName,
    string Username,
    UserRoleDto Role,
    bool MustChangePassword);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
