using OzunuInkisaf.Contracts.Khatim;

namespace OzunuInkisaf.Application.Services;

public interface IKhatimService
{
    Task<KhatimCycleStatusDto2> GetCurrentStatusAsync(Guid currentUserId, bool isAdmin, CancellationToken cancellationToken = default);

    Task<KhatimCycleStatusDto2> ClaimJuzAsync(Guid userId, ClaimJuzRequest request, bool isAdmin, CancellationToken cancellationToken = default);

    Task<KhatimCycleStatusDto2> CompleteJuzAsync(Guid userId, CompleteJuzRequest request, bool isAdmin, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KhatimHistoryItemDto>> GetHistoryAsync(CancellationToken cancellationToken = default);
}
