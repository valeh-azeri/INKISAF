using OzunuInkisaf.Domain.Common;

namespace OzunuInkisaf.Domain.Entities;

/// <summary>
/// A record of the admin declaring a winner for a given period, optionally
/// scoped to one or more PointsSource categories (book reading, Khatim,
/// tally). CategoriesCsv holds the selected PointsSource enum values as
/// comma-separated ints, or is null/empty for "all categories".
/// </summary>
public class WinnerAward : BaseEntity
{
    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public string? CategoriesCsv { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public int TotalScore { get; set; }

    public DateTime AnnouncedAt { get; set; } = DateTime.UtcNow;

    public Guid AnnouncedByUserId { get; set; }

    public User? AnnouncedBy { get; set; }
}
