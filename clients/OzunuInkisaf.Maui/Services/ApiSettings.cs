namespace OzunuInkisaf.Maui.Services;

/// <summary>
/// Backend API-nin ünvanı. Platformdan asılı olaraq "localhost" fərqli
/// mənalar daşıdığı üçün burada compile-time şərtlə seçilir. Production-a
/// keçəndə bunu real server domeninizlə əvəz edin (məsələn Azure App Service
/// ünvanı) — ən sadəsi bunu bir dəfə burada dəyişməkdir.
/// </summary>
public static class ApiSettings
{
    public static string BaseUrl =>
#if ANDROID
        "https://10.0.2.2:7001/";
#elif IOS || MACCATALYST
        "https://localhost:7001/";
#else
        "https://localhost:7001/";
#endif
}
