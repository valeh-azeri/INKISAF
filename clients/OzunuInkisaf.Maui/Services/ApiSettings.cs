namespace OzunuInkisaf.Maui.Services;

/// <summary>
/// Backend API-nin ünvanı. Platformdan asılı olaraq "localhost" fərqli
/// mənalar daşıdığı üçün burada compile-time şərtlə seçilir. Production-a
/// keçəndə bunu real server domeninizlə əvəz edin (məsələn Azure App Service
/// ünvanı) — ən sadəsi bunu bir dəfə burada dəyişməkdir.
/// </summary>
public static class ApiSettings
{
    public static string BaseUrl => "https://ozunuinkisaf-api.azurewebsites.net/";
}
