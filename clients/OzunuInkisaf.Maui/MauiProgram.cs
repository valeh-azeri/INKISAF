using Microsoft.Extensions.Logging;
using OzunuInkisaf.Maui.Services;

namespace OzunuInkisaf.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        // Qeyd: MainPage tamamilə BlazorWebView-dan ibarət olduğu üçün UI
        // fontları wwwroot/css/app.css-də Google Fonts (Lora/Manrope) ilə
        // idarə olunur — burada ayrıca .ttf qoşmağa ehtiyac yoxdur. İstəsəniz
        // Resources/Fonts qovluğuna öz .ttf faylınızı əlavə edib
        // ConfigureFonts(...) ilə qeydiyyatdan keçirə bilərsiniz.

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // ---- Backend bağlantısı --------------------------------------------
        // ApiSettings.BaseUrl-i öz mühitinizə uyğun dəyişin:
        //   - Windows-da yerli test: https://localhost:7001/
        //   - Android emulyator: https://10.0.2.2:7001/ (emulyator "localhost"u
        //     öz özünə yönləndirir, ona görə host maşınına bu ünvanla çatılır)
        //   - Real cihaz / production: serverin öz domeni/IP-si
        builder.Services.AddSingleton(sp => new HttpClient
        {
            BaseAddress = new Uri(ApiSettings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(60),
        });

        builder.Services.AddSingleton<AuthState>();
        builder.Services.AddSingleton<ApiClient>();

        return builder.Build();
    }
}
