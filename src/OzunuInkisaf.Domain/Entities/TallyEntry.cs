using OzunuInkisaf.Domain.Common;

namespace OzunuInkisaf.Domain.Entities;

/// <summary>
/// One user's completion mark for one TallyItem on one specific date. A
/// week's worth of these is what renders as the 7-day tracker in the UI.
/// </summary>
public class TallyEntry : BaseEntity
{
    public Guid TallyItemId { get; set; }

    public TallyItem? TallyItem { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public DateOnly Date { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }
}
