using OzunuInkisaf.Domain.Common;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Domain.Entities;

/// <summary>
/// One full community Khatim (Qur'an completion) round. A cycle always has
/// exactly 30 JuzClaim rows (one per juz'). When every juz' is marked
/// completed, the cycle is closed and a new one (CycleNumber + 1) opens.
/// </summary>
public class KhatimCycle : BaseEntity
{
    public int CycleNumber { get; set; }

    public KhatimCycleStatus Status { get; set; } = KhatimCycleStatus.InProgress;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public ICollection<JuzClaim> JuzClaims { get; set; } = new List<JuzClaim>();
}
