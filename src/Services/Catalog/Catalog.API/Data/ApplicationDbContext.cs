using Catalog.API.Services;
using Catalog.Domain.Api.Requests.Enums;

namespace Catalog.API.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IPlateFuzzySearcher _plateFuzzySearcher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IPlateFuzzySearcher plateFuzzySearcher)
        : base(options)
    {
        _plateFuzzySearcher = plateFuzzySearcher;
    }
    
    public DbSet<Plate> Plates { get; set; }

    public async Task<List<Plate>> GetPlates(
        int pageNumber,
        int pageSize,
        SortBy sortBy = SortBy.Alphabetical,
        OrderBy orderBy = OrderBy.Asc,
        string? searchTerm = null)
    {
        var searchedPlates = string.IsNullOrWhiteSpace(searchTerm)
            ? Plates
            : Plates.Where(p => _plateFuzzySearcher.IsMatch(p.Registration, searchTerm));
        
        var orderedPlates = OrderPlatesBy(searchedPlates, sortBy, orderBy);
        
        return await orderedPlates
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<int> GetTotalCount()
    {
        return await Plates.CountAsync();
    }

    private static IOrderedQueryable<Plate> OrderPlatesBy(IQueryable<Plate> plates, SortBy sortBy, OrderBy orderBy)
    {
        return (sortBy, orderBy) switch
        {
            (SortBy.Alphabetical, OrderBy.Asc) => plates.OrderBy(p => p.Registration),
            (SortBy.Price, OrderBy.Asc) => plates.OrderBy(p => p.SalePrice),
            (SortBy.Alphabetical, OrderBy.Desc) => plates.OrderByDescending(p => p.Registration),
            (SortBy.Price, OrderBy.Desc) => plates.OrderByDescending(p => p.SalePrice),
            _ => plates.OrderBy(p => p.Registration)
        };
    }
}
