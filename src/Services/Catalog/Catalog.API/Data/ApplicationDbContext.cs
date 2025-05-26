namespace Catalog.API.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public List<Plate> GetPlates(int pageNumber, int pageSize)
    {
        return Plates
            .OrderBy(p => p.Registration)
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToList();
    }

    public DbSet<Plate> Plates { get; set; }
}
