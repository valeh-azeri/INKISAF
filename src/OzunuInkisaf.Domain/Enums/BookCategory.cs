namespace OzunuInkisaf.Domain.Enums;

/// <summary>
/// A Book row is used both for regular monthly reading assignments and for
/// the Qur'an text itself — they are the same "PDF with tracked reading
/// position" concept, distinguished only by category.
/// </summary>
public enum BookCategory
{
    General = 0,
    Quran = 1
}

public enum BookStatus
{
    Active = 0,
    Archived = 1
}
