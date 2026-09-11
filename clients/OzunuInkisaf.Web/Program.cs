using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OzunuInkisaf.Web.Components;
using OzunuInkisaf.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Backend ünvanı — MAUI client ilə eyni Azure App Service.
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://ozunuinkisaf-api.azurewebsites.net/"),
    Timeout = TimeSpan.FromSeconds(60),
});

builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<ApiClient>();

await builder.Build().RunAsync();
