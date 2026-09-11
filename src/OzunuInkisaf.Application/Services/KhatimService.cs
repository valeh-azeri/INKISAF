using Microsoft.EntityFrameworkCore;
using OzunuInkisaf.Application.Common.Exceptions;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Contracts.Khatim;
using OzunuInkisaf.Domain.Entities;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Application.Services;

public class KhatimService : IKhatimService
{
    public const int TotalJuz = 30;
    private const int PointsPerJuzCompleted = 15;

    private readonly IApplicationDbContext _db;

    public KhatimService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<KhatimCycleStatusDto2> GetCurrentStatusAsync(Guid currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var cycle = await GetOrCreateCurrentCycleAsync(cancellationToken);
        return await BuildStatusDtoAsync(cycle, currentUserId, isAdmin, cancellationToken);
    }

    public async Task<KhatimCycleStatusDto2> ClaimJuzAsync(Guid userId, ClaimJuzRequest request, bool isAdmin, CancellationToken cancellationToken = default)
    {
        ValidateJuzNumber(request.JuzNumber);

        var cycle = await GetOrCreateCurrentCycleAsync(cancellationToken);

        var claim = cycle.JuzClaims.Single(j => j.JuzNumber == request.JuzNumber);

        if (claim.UserId is not null)
        {
            throw new ConflictException($"{request.JuzNumber}-ci cüz artıq götürülüb.");
        }

        var alreadyHasClaim = cycle.JuzClaims.Any(j => j.UserId == userId && !j.IsCompleted);
        if (alreadyHasClaim)
        {
            throw new ConflictException("Bu dövrdə artıq bir cüzünüz var. Əvvəlcə onu tamamlayın.");
        }

        claim.UserId = userId;
        claim.ClaimedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return await BuildStatusDtoAsync(cycle, userId, isAdmin, cancellationToken);
    }

    public async Task<KhatimCycleStatusDto2> CompleteJuzAsync(Guid userId, CompleteJuzRequest request, bool isAdmin, CancellationToken cancellationToken = default)
    {
        ValidateJuzNumber(request.JuzNumber);

        var cycle = await GetOrCreateCurrentCycleAsync(cancellationToken);
        var claim = cycle.JuzClaims.Single(j => j.JuzNumber == request.JuzNumber);

        if (claim.UserId != userId)
        {
            throw new ForbiddenAccessException("Bu cüz sizə aid deyil.");
        }

        if (!claim.IsCompleted)
        {
            claim.IsCompleted = true;
            claim.CompletedAt = DateTime.UtcNow;

            _db.PointsTransactions.Add(new PointsTransaction
            {
                UserId = userId,
                Source = PointsSource.KhatimJuzCompleted,
                SourceId = claim.Id,
                Points = PointsPerJuzCompleted,
            });

            await _db.SaveChangesAsync(cancellationToken);

            if (cycle.JuzClaims.All(j => j.IsCompleted))
            {
                cycle.Status = KhatimCycleStatus.Completed;
                cycle.CompletedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);

                cycle = await CreateNewCycleAsync(cycle.CycleNumber + 1, cancellationToken);
            }
        }

        return await BuildStatusDtoAsync(cycle, userId, isAdmin, cancellationToken);
    }

    public async Task<IReadOnlyList<KhatimHistoryItemDto>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        var cycles = await _db.KhatimCycles
            .Include(c => c.JuzClaims)
            .OrderByDescending(c => c.CycleNumber)
            .ToListAsync(cancellationToken);

        return cycles.Select(c => new KhatimHistoryItemDto(
            c.CycleNumber,
            c.StartedAt,
            c.CompletedAt,
            c.JuzClaims.Where(j => j.UserId != null).Select(j => j.UserId!.Value).Distinct().Count())).ToList();
    }

    private async Task<KhatimCycle> GetOrCreateCurrentCycleAsync(CancellationToken cancellationToken)
    {
        var cycle = await _db.KhatimCycles
            .Include(c => c.JuzClaims)
            .Where(c => c.Status == KhatimCycleStatus.InProgress)
            .OrderByDescending(c => c.CycleNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (cycle is not null)
        {
            return cycle;
        }

        var lastCycleNumber = await _db.KhatimCycles
            .OrderByDescending(c => c.CycleNumber)
            .Select(c => c.CycleNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return await CreateNewCycleAsync(lastCycleNumber + 1, cancellationToken);
    }

    private async Task<KhatimCycle> CreateNewCycleAsync(int cycleNumber, CancellationToken cancellationToken)
    {
        var cycle = new KhatimCycle
        {
            CycleNumber = cycleNumber,
            Status = KhatimCycleStatus.InProgress,
            StartedAt = DateTime.UtcNow,
        };

        for (var juzNumber = 1; juzNumber <= TotalJuz; juzNumber++)
        {
            cycle.JuzClaims.Add(new JuzClaim
            {
                KhatimCycleId = cycle.Id,
                JuzNumber = juzNumber,
            });
        }

        _db.KhatimCycles.Add(cycle);
        await _db.SaveChangesAsync(cancellationToken);

        return cycle;
    }

    private async Task<KhatimCycleStatusDto2> BuildStatusDtoAsync(KhatimCycle cycle, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var userIds = cycle.JuzClaims.Where(j => j.UserId != null).Select(j => j.UserId!.Value).Distinct().ToList();
        var users = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var completedCyclesCount = await _db.KhatimCycles.CountAsync(c => c.Status == KhatimCycleStatus.Completed, cancellationToken);

        var juzDtos = cycle.JuzClaims
            .OrderBy(j => j.JuzNumber)
            .Select(j =>
            {
                var isOwnClaim = j.UserId == currentUserId;
                var canSeeIdentity = isAdmin || isOwnClaim;

                string? fullName = null;
                string? initials = null;
                if (canSeeIdentity && j.UserId is not null && users.TryGetValue(j.UserId.Value, out var user))
                {
                    fullName = user.FullName;
                    initials = BuildInitials(user.FullName);
                }

                return new JuzClaimDto(
                    j.JuzNumber,
                    j.UserId,
                    fullName,
                    initials,
                    isOwnClaim,
                    j.IsCompleted,
                    j.PageRangeStart,
                    j.PageRangeEnd);
            })
            .ToList();

        return new KhatimCycleStatusDto2(
            cycle.Id,
            cycle.CycleNumber,
            (Contracts.Common.KhatimCycleStatusDto)cycle.Status,
            cycle.JuzClaims.Count(j => j.IsCompleted),
            cycle.JuzClaims.Count(j => j.UserId != null),
            completedCyclesCount,
            juzDtos);
    }

    private static void ValidateJuzNumber(int juzNumber)
    {
        if (juzNumber is < 1 or > TotalJuz)
        {
            throw new ValidationAppException($"Cüz nömrəsi 1 ilə {TotalJuz} arasında olmalıdır.");
        }
    }

    private static string BuildInitials(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
    }
}
