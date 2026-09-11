using OzunuInkisaf.Domain.Common;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Domain.Entities;

/// <summary>
/// Append-only audit trail of every point a user has earned. A user's total
/// score is the sum of their transactions — never stored as a single mutable
/// counter — so reports can be sliced by period and by category
/// (PointsSource) without recomputation ambiguity.
/// </summary>
public class PointsTransaction : BaseEntity
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public PointsSource Source { get; set; }

    /// <summary>Id of the TallyEntry, JuzClaim or Book that generated these points.</summary>
    public Guid SourceId { get; set; }

    public int Points { get; set; }

    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
}
