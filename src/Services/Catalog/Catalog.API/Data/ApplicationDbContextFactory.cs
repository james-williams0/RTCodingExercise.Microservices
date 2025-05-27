using Catalog.API.Services;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.API.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private readonly IPlateFuzzySearcher _plateFuzzySearcher;

    public ApplicationDbContextFactory(IPlateFuzzySearcher plateFuzzySearcher)
    {
        _plateFuzzySearcher = plateFuzzySearcher;
    }
    
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(config["ConnectionString"], sqlServerOptionsAction: o => o.MigrationsAssembly("Catalog.API"));

        return new ApplicationDbContext(optionsBuilder.Options, _plateFuzzySearcher);
    }
}