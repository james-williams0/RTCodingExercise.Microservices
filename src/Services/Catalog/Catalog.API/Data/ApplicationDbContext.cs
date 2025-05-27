using Catalog.Domain.Api.Requests.Enums;

namespace Catalog.API.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    
    public DbSet<Plate> Plates { get; set; }

    public async Task<List<Plate>> GetPlates(
        int pageNumber,
        int pageSize,
        SortBy sortBy = SortBy.Alphabetical,
        OrderBy orderBy = OrderBy.Asc)
    {
        var orderedPlates = OrderPlatesBy(sortBy, orderBy);
        
        return await orderedPlates
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<int> GetTotalCount()
    {
        return await Plates.CountAsync();
    }

    private IOrderedQueryable<Plate> OrderPlatesBy(SortBy sortBy, OrderBy orderBy)
    {
        return (sortBy, orderBy) switch
        {
            (SortBy.Alphabetical, OrderBy.Asc) => Plates.OrderBy(p => p.Registration),
            (SortBy.Price, OrderBy.Asc) => Plates.OrderBy(p => p.SalePrice),
            (SortBy.Alphabetical, OrderBy.Desc) => Plates.OrderByDescending(p => p.Registration),
            (SortBy.Price, OrderBy.Desc) => Plates.OrderByDescending(p => p.SalePrice),
            _ => Plates.OrderBy(p => p.Registration)
        };
    }
}
