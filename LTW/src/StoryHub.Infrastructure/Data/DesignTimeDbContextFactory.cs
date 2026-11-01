using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace StoryHub.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<StoryHubDbContext>
{
    public StoryHubDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../StoryHub.Web");
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=StoryHubDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var builder = new DbContextOptionsBuilder<StoryHubDbContext>();
        builder.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(StoryHubDbContext).Assembly.FullName));

        return new StoryHubDbContext(builder.Options);
    }
}
