using System.Security.Cryptography;
using System.Text;

namespace OzunuInkisaf.Application.Common;

/// <summary>
/// Turns "Aygün Məmmədova" into a clean "aygun.mammadova" username and can
/// mint random, reasonably strong passwords for admin-generated accounts.
/// </summary>
public static class CredentialGenerator
{
    private static readonly Dictionary<char, char> AzToAscii = new()
    {
        ['ə'] = 'e', ['Ə'] = 'e',
        ['ı'] = 'i', ['I'] = 'i',
        ['İ'] = 'i',
        ['ö'] = 'o', ['Ö'] = 'o',
        ['ü'] = 'u', ['Ü'] = 'u',
        ['ç'] = 'c', ['Ç'] = 'c',
        ['ş'] = 's', ['Ş'] = 's',
        ['ğ'] = 'g', ['Ğ'] = 'g',
    };

    public static string Slugify(string text)
    {
        var sb = new StringBuilder();
        foreach (var ch in text.Trim())
        {
            if (AzToAscii.TryGetValue(ch, out var mapped))
            {
                sb.Append(mapped);
            }
            else if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
            else if (char.IsWhiteSpace(ch) && sb.Length > 0 && sb[^1] != '.')
            {
                sb.Append('.');
            }
        }

        return sb.ToString().Trim('.');
    }

    public static string BuildUsernameBase(string fullName)
    {
        var slug = Slugify(fullName);
        return string.IsNullOrWhiteSpace(slug) ? "istifadeci" : slug;
    }

    /// <summary>An 8-character password with upper/lower/digit/symbol, e.g. "K7mR#42x".</summary>
    public static string GenerateRandomPassword(int length = 8)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%";
        const string all = upper + lower + digits + symbols;

        Span<char> result = stackalloc char[length];
        result[0] = PickRandom(upper);
        result[1] = PickRandom(lower);
        result[2] = PickRandom(digits);
        result[3] = PickRandom(symbols);

        for (var i = 4; i < length; i++)
        {
            result[i] = PickRandom(all);
        }

        // Shuffle so the guaranteed classes aren't always in the same position.
        for (var i = result.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return new string(result);
    }

    private static char PickRandom(string charset) => charset[RandomNumberGenerator.GetInt32(charset.Length)];
}
