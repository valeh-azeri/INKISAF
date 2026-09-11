using OzunuInkisaf.Domain.Common;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Domain.Entities;

/// <summary>
/// One line of a TallyTemplate, e.g. "Daily Qur'an reading — min. 1 page"
/// or "Fasting — Monday and Thursday". SpecificDaysCsv is only used when
/// Frequency == SpecificDays (day-of-week ints, comma separated, 1=Monday).
/// </summary>
public class TallyItem : BaseEntity
{
    public Guid TallyTemplateId { get; set; }

    public TallyTemplate? TallyTemplate { get; set; }

    public string Title { get; set; } = string.Empty;

    public TallyItemFrequency Frequency { get; set; } = TallyItemFrequency.Daily;

    /// <summary>Comma-separated ISO day-of-week numbers (1=Monday..7=Sunday). Null/empty when Frequency == Daily.</summary>
    public string? SpecificDaysCsv { get; set; }

    public int PointsPerCompletion { get; set; } = 5;

    public int DisplayOrder { get; set; }

    public ICollection<TallyEntry> Entries { get; set; } = new List<TallyEntry>();
}
