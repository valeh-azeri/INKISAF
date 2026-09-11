using OzunuInkisaf.Domain.Common;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Domain.Entities;

/// <summary>
/// A PDF the admin has placed in the library — either a regular monthly
/// reading assignment or the Qur'an itself (Category = Quran). Both are read
/// through the same reader flow with a saved page position.
/// </summary>
public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Author { get; set; }

    public BookCategory Category { get; set; } = BookCategory.General;

    public BookStatus Status { get; set; } = BookStatus.Active;

    /// <summary>Relative storage path/URL of the uploaded PDF file.</summary>
    public string FilePath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public int TotalPages { get; set; }

    /// <summary>Free-text "assigned month" label, e.g. "Sentyabr 2026", set when this is the current month's book.</summary>
    public string? AssignedMonthLabel { get; set; }

    public bool IsCurrentMonthPick { get; set; }

    public Guid UploadedByUserId { get; set; }

    public User? UploadedBy { get; set; }

    public ICollection<ReadingProgress> ReadingProgresses { get; set; } = new List<ReadingProgress>();
}
