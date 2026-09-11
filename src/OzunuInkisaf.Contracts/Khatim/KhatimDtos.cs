using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.Contracts.Khatim;

public record JuzClaimDto(
    int JuzNumber,
    Guid? UserId,
    string? UserFullName,
    string? UserInitials,
    bool IsClaimedByCurrentUser,
    bool IsCompleted,
    int? PageRangeStart,
    int? PageRangeEnd);

public record KhatimCycleStatusDto2(
    Guid CycleId,
    int CycleNumber,
    KhatimCycleStatusDto Status,
    int CompletedJuzCount,
    int ClaimedJuzCount,
    int TotalPreviouslyCompletedCycles,
    IReadOnlyList<JuzClaimDto> Juz);

public record ClaimJuzRequest(int JuzNumber);

public record CompleteJuzRequest(int JuzNumber);

public record KhatimHistoryItemDto(int CycleNumber, DateTime StartedAt, DateTime? CompletedAt, int ParticipantCount);
