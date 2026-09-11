using OzunuInkisaf.Domain.Common;

namespace OzunuInkisaf.Domain.Entities;

/// <summary>
/// One of the 30 juz' slots inside a KhatimCycle. Unclaimed rows have
/// UserId == null. A juz' is "taken" once claimed and "read" once the
/// claiming user marks it IsCompleted.
/// </summary>
public class JuzClaim : BaseEntity
{
    public Guid KhatimCycleId { get; set; }

    public KhatimCycle? KhatimCycle { get; set; }

    /// <summary>1 through 30.</summary>
    public int JuzNumber { get; set; }

    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public DateTime? ClaimedAt { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? PageRangeStart { get; set; }

    public int? PageRangeEnd { get; set; }
}
