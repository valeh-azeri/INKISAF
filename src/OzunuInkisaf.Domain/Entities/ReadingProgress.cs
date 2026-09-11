using OzunuInkisaf.Domain.Common;

namespace OzunuInkisaf.Domain.Entities;

/// <summary>
/// Tracks exactly one user's position inside exactly one book (or the
/// Qur'an, which is just a Book with Category = Quran). This is what lets a
/// reader close the app and reopen a PDF exactly where they left off.
/// </summary>
public class ReadingProgress : BaseEntity
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    public Guid BookId { get; set; }

    public Book? Book { get; set; }

    public int CurrentPage { get; set; } = 1;

    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }
}
