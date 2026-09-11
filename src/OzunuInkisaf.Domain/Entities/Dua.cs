using OzunuInkisaf.Domain.Common;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Domain.Entities;

/// <summary>
/// A short supplication the admin publishes for on-demand reading (not part
/// of the monthly-book flow and not tracked with a reading position).
/// </summary>
public class Dua : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string ArabicText { get; set; } = string.Empty;

    public string Translation { get; set; } = string.Empty;

    public string? Transliteration { get; set; }

    public DuaCategory Category { get; set; } = DuaCategory.Daily;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid CreatedByUserId { get; set; }

    public User? CreatedBy { get; set; }
}
