using GardensBack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GardensBack.Infrastructure.Persistance
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            string settingsDirectory = FindSettingsDirectory();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(settingsDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            string appSettingsConnString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection.");

            string connString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? appSettingsConnString;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>();

            InfrastructureDependencyInjection.ConfigureDatabase(options, connString);

            return new ApplicationDbContext(options.Options);
        }

        private static string FindSettingsDirectory()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string[] candidates =
            [
                currentDirectory,
                Path.Combine(currentDirectory, "GardensBack"),
                Path.GetFullPath(Path.Combine(currentDirectory, "..", "GardensBack"))
            ];

            return candidates.FirstOrDefault(path => File.Exists(Path.Combine(path, "appsettings.json")))
                ?? throw new DirectoryNotFoundException(
                    $"No se encontró appsettings.json desde '{currentDirectory}'.");
        }
    }
}
