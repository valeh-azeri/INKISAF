using System.Security.Cryptography;
using OzunuInkisaf.Application.Common.Interfaces;

namespace OzunuInkisaf.Infrastructure.Security;

/// <summary>
/// PBKDF2 (Rfc2898) password hashing using only .NET's built-in
/// System.Security.Cryptography — deliberately no third-party package, so
/// there is nothing extra to restore.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public (string Hash, string Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSizeBytes);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var expectedHash = Convert.FromBase64String(hash);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, Algorithm, HashSizeBytes);

        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }
}
