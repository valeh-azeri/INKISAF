using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.Contracts.Tally;

public record TallyItemDto(
    Guid Id,
    string Title,
    TallyItemFrequencyDto Frequency,
    IReadOnlyList<int> SpecificDays,
    int PointsPerCompletion);

public record TallyDayEntryDto(DateOnly Date, bool IsCompleted);

public record TallyItemWithEntriesDto(TallyItemDto Item, IReadOnlyList<TallyDayEntryDto> Entries);

public record TallyTemplateDto(
    Guid Id,
    string Title,
    DateOnly WeekStartDate,
    DateOnly WeekEndDate,
    DateOnly DueDate,
    TallyStatusDto Status,
    int TotalPointsThisWeek,
    IReadOnlyList<TallyItemWithEntriesDto> Items);

public record CreateTallyTemplateRequest(
    string Title,
    DateOnly WeekStartDate,
    DateOnly WeekEndDate,
    DateOnly DueDate,
    IReadOnlyList<CreateTallyItemRequest> Items);

public record CreateTallyItemRequest(
    string Title,
    TallyItemFrequencyDto Frequency,
    IReadOnlyList<int> SpecificDays,
    int PointsPerCompletion);

public record SetTallyEntryRequest(Guid TallyItemId, DateOnly Date, bool IsCompleted);

public record TallyTemplateSummaryDto(
    Guid Id,
    string Title,
    DateOnly WeekStartDate,
    DateOnly WeekEndDate,
    TallyStatusDto Status,
    double ParticipationRate);
