using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OzunuInkisaf.Application.Common.Interfaces;
using OzunuInkisaf.Application.Services;
using OzunuInkisaf.Infrastructure.Persistence;
using OzunuInkisaf.Infrastructure.Security;
using OzunuInkisaf.Infrastructure.Storage;

namespace OzunuInkisaf.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection appsettings.json-də tapılmadı.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        var pdfStoragePath = configuration["Storage:PdfRootPath"] ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "pdfs");
        services.AddSingleton<IFileStorageService>(new LocalFileStorageService(pdfStoragePath));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IDuaService, DuaService>();
        services.AddScoped<IKhatimService, KhatimService>();
        services.AddScoped<ITallyService, TallyService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
