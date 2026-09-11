using OzunuInkisaf.Domain.Common;
using OzunuInkisaf.Domain.Enums;

namespace OzunuInkisaf.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string PasswordSalt { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// True until the user changes the password they were issued by the
    /// admin. The client can use this to nudge a first-time password change.
    /// </summary>
    public bool MustChangePassword { get; set; } = true;

    public DateTime? LastActiveAt { get; set; }

    public ICollection<ReadingProgress> ReadingProgresses { get; set; } = new List<ReadingProgress>();

    public ICollection<JuzClaim> JuzClaims { get; set; } = new List<JuzClaim>();

    public ICollection<TallyEntry> TallyEntries { get; set; } = new List<TallyEntry>();

    public ICollection<PointsTransaction> PointsTransactions { get; set; } = new List<PointsTransaction>();
}
