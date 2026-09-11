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

    /// <summary>Yazılı mətn yolu seçildikdə doldurulur — PDF-lə birgə də ola bilər.</summary>
    public string? ArabicText { get; set; }

    public string? Translation { get; set; }

    public string? Transliteration { get; set; }

    /// <summary>Admin mətn yerinə (və ya əlavə olaraq) PDF yükləyibsə, onun saxlanma yolu.</summary>
    public string? PdfPath { get; set; }

    public long? PdfFileSizeBytes { get; set; }

    public DuaCategory Category { get; set; } = DuaCategory.Daily;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid CreatedByUserId { get; set; }

    public User? CreatedBy { get; set; }
}
