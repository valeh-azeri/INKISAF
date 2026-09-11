namespace OzunuInkisaf.Infrastructure.Security;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "OzunuInkisaf";

    public string Audience { get; set; } = "OzunuInkisafClients";

    /// <summary>
    /// Must be overridden with a long, random value in appsettings — this
    /// default is only here so the project still runs out of the box in
    /// development. See README.
    /// </summary>
    public string Secret { get; set; } = "CHANGE_ME_development_only_secret_key_min_32_chars!";

    public int ExpiryMinutes { get; set; } = 60 * 12;
}
