namespace Catalog.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public List<Plate> GetPlates(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public DbSet<Plate> Plates { get; set; }
}
