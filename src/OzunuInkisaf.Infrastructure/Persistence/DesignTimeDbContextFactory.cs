using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OzunuInkisaf.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations add / database update` construct the
/// DbContext WITHOUT running the full WebApi host. Run these commands from
/// the WebApi project so appsettings.json (and its connection string) is
/// picked up, e.g.:
///
///   cd src/OzunuInkisaf.WebApi
///   dotnet ef migrations add InitialCreate --project ../OzunuInkisaf.Infrastructure --startup-project .
///   dotnet ef database update --project ../OzunuInkisaf.Infrastructure --startup-project .
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=OzunuInkisaf;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
