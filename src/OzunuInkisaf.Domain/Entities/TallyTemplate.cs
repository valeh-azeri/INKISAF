using OzunuInkisaf.Domain.Common;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Domain.Entities;

/// <summary>
/// "Çətələ" — a weekly checklist of religious minimums (e.g. daily Qur'an
/// reading, Monday/Thursday fasting, tahajjud) the admin defines and
/// publishes. Every user sees the same published template and checks off
/// their own TallyEntry rows against it.
/// </summary>
public class TallyTemplate : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public DateOnly WeekStartDate { get; set; }

    public DateOnly WeekEndDate { get; set; }

    /// <summary>The day users must have their checklist finalized by.</summary>
    public DateOnly DueDate { get; set; }

    public TallyStatus Status { get; set; } = TallyStatus.Draft;

    public Guid CreatedByUserId { get; set; }

    public User? CreatedBy { get; set; }

    public DateTime? PublishedAt { get; set; }

    public ICollection<TallyItem> Items { get; set; } = new List<TallyItem>();
}
