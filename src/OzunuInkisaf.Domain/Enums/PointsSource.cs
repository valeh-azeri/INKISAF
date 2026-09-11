namespace OzunuInkisaf.Domain.Enums;

/// <summary>
/// Where a points transaction came from. Used both to audit scoring and to
/// let the admin filter reports/leaderboards "by category" (book reading vs
/// Khatim participation vs tally/checklist points), as requested for the
/// winner-selection feature.
/// </summary>
public enum PointsSource
{
    TallyEntry = 0,
    KhatimJuzCompleted = 1,
    BookCompleted = 2
}
