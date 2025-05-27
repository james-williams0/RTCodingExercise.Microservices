using Catalog.Domain.Api.Requests.Enums;

namespace Catalog.API.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public async Task<List<Plate>> GetPlates(
        int pageNumber,
        int pageSize,
        SortBy sortBy = SortBy.Alphabetical,
        OrderBy orderBy = OrderBy.Asc)
    {
        return await Plates
            .OrderBy(p => p.Registration)
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCount()
    {
        return await Plates.CountAsync();
    }

    public DbSet<Plate> Plates { get; set; }
}
