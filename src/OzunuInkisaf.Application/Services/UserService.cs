using Microsoft.EntityFrameworkCore;
using OzunuInkisaf.Application.Common;
using OzunuInkisaf.Application.Common.Exceptions;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Common;
using OzunuInkisaf.Contracts.Users;
using OzunuInkisaf.Domain.Entities;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Application.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IApplicationDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetAllAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = _db.Users.Where(u => u.Role == UserRole.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Username.ToLower().Contains(term));
        }

        var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync(cancellationToken);
        var userIds = users.Select(u => u.Id).ToList();

        var pointsByUser = await _db.PointsTransactions
            .Where(p => userIds.Contains(p.UserId))
            .GroupBy(p => p.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(x => x.Points) })
            .ToDictionaryAsync(x => x.UserId, x => x.Total, cancellationToken);

        return users.Select(u => new UserSummaryDto(
            u.Id,
            u.FullName,
            u.Username,
            u.Email,
            (UserRoleDto)u.Role,
            u.IsActive,
            u.CreatedAt,
            u.LastActiveAt,
            pointsByUser.GetValueOrDefault(u.Id, 0))).ToList();
    }

    public async Task<CreateUserResult> CreateAsync(CreateUserRequest request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ValidationAppException("Ad Soyad tələb olunur.");
        }

        var username = string.IsNullOrWhiteSpace(request.UsernameOverride)
            ? CredentialGenerator.BuildUsernameBase(request.FullName)
            : CredentialGenerator.Slugify(request.UsernameOverride);

        username = await EnsureUniqueUsernameAsync(username, cancellationToken);

        var password = string.IsNullOrWhiteSpace(request.PasswordOverride)
            ? CredentialGenerator.GenerateRandomPassword()
            : request.PasswordOverride!;

        var (hash, salt) = _passwordHasher.HashPassword(password);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Username = username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = UserRole.User,
            MustChangePassword = true,
            IsActive = true,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        // NOTE: actually emailing/SMS-ing the credentials to request.DeliveryEmail
        // is an Infrastructure concern (a notification/email sender). Wire an
        // INotificationService in here once you pick an email/SMS provider —
        // deliberately left out so this template doesn't ship a fake integration.

        return new CreateUserResult(user.Id, user.FullName, user.Username, password);
    }

    public async Task<BulkCreateUsersResult> BulkCreateAsync(int count, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        if (count is < 1 or > 500)
        {
            throw new ValidationAppException("Say 1 ilə 500 arasında olmalıdır.");
        }

        var results = new List<CreateUserResult>();
        for (var i = 0; i < count; i++)
        {
            var placeholderName = $"Yeni İstifadəçi {DateTime.UtcNow:yyMMddHHmmss}-{i + 1}";
            var result = await CreateAsync(
                new CreateUserRequest(placeholderName, null, null, null, null, null),
                createdByUserId,
                cancellationToken);
            results.Add(result);
        }

        return new BulkCreateUsersResult(results);
    }

    public async Task<BulkCreateUsersResult> BulkCreateFromListAsync(BulkCreateUsersRequestFromList request, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        var results = new List<CreateUserResult>();
        foreach (var row in request.Users)
        {
            var result = await CreateAsync(
                new CreateUserRequest(row.FullName, row.Email, row.PhoneNumber, null, null, row.Email),
                createdByUserId,
                cancellationToken);
            results.Add(result);
        }

        return new BulkCreateUsersResult(results);
    }

    public async Task SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        user.IsActive = isActive;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<CreateUserResult> ResetPasswordAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        var password = CredentialGenerator.GenerateRandomPassword();
        var (hash, salt) = _passwordHasher.HashPassword(password);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;
        user.MustChangePassword = true;

        await _db.SaveChangesAsync(cancellationToken);

        return new CreateUserResult(user.Id, user.FullName, user.Username, password);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        if (user.Role == UserRole.Admin)
        {
            throw new ValidationAppException("Admin hesabı silinə bilməz.");
        }

        var readingProgresses = await _db.ReadingProgresses.Where(r => r.UserId == userId).ToListAsync(cancellationToken);
        _db.ReadingProgresses.RemoveRange(readingProgresses);

        var tallyEntries = await _db.TallyEntries.Where(e => e.UserId == userId).ToListAsync(cancellationToken);
        _db.TallyEntries.RemoveRange(tallyEntries);

        var pointsTransactions = await _db.PointsTransactions.Where(p => p.UserId == userId).ToListAsync(cancellationToken);
        _db.PointsTransactions.RemoveRange(pointsTransactions);

        var juzClaims = await _db.JuzClaims.Where(j => j.UserId == userId).ToListAsync(cancellationToken);
        foreach (var claim in juzClaims)
        {
            claim.UserId = null;
            claim.ClaimedAt = null;
            claim.IsCompleted = false;
            claim.CompletedAt = null;
            claim.PageRangeStart = null;
            claim.PageRangeEnd = null;
        }

        _db.Users.Remove(user);

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> EnsureUniqueUsernameAsync(string baseUsername, CancellationToken cancellationToken)
    {
        var candidate = baseUsername;
        var suffix = 1;

        while (await _db.Users.AnyAsync(u => u.Username == candidate, cancellationToken))
        {
            suffix++;
            candidate = $"{baseUsername}{suffix}";
        }

        return candidate;
    }
}
