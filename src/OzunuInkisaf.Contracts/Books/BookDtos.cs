using OzunuInkisaf.Contracts.Common;

namespace OzunuInkisaf.Contracts.Books;

public record BookSummaryDto(
    Guid Id,
    string Title,
    string? Author,
    BookCategoryDto Category,
    BookStatusDto Status,
    int TotalPages,
    string? AssignedMonthLabel,
    bool IsCurrentMonthPick,
    DateTime CreatedAt,
    int? CurrentUserPage,
    bool? CurrentUserCompleted);

public record UploadBookRequest(
    string Title,
    string? Author,
    BookCategoryDto Category,
    string? AssignedMonthLabel,
    bool IsCurrentMonthPick,
    bool NotifyAllUsers,
    int? TotalPages = null);

public record SaveReadingProgressRequest(int CurrentPage);

public record ReadingProgressDto(Guid BookId, int CurrentPage, int TotalPages, bool IsCompleted, DateTime LastReadAt);
