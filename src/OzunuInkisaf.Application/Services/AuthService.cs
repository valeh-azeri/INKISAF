using Microsoft.EntityFrameworkCore;
using OzunuInkisaf.Application.Common.Exceptions;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Auth;
using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IApplicationDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim().ToLowerInvariant();

        var user = await _db.Users
            .SingleOrDefaultAsync(u => u.Username.ToLower() == username, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new AuthenticationFailedException();
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new AuthenticationFailedException();
        }

        user.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return new LoginResponse(
            token,
            expiresAt,
            user.Id,
            user.FullName,
            user.Username,
            (UserRoleDto)user.Role,
            user.MustChangePassword);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.User), userId);

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
        {
            throw new ValidationAppException("Hazırkı şifrə yanlışdır.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            throw new ValidationAppException("Yeni şifrə ən azı 6 simvol olmalıdır.");
        }

        var (hash, salt) = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.MustChangePassword = false;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
