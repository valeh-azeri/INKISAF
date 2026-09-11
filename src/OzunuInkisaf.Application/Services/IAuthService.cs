using OzunuInkisaf.Contracts.Auth;

namespace OzunuInkisaf.Application.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>Admin-in bir istifadəçinin hesabına, şifrəsini bilmədən, birbaşa daxil olması üçün token verir.</summary>
    Task<LoginResponse> ImpersonateAsync(Guid targetUserId, CancellationToken cancellationToken = default);
}
