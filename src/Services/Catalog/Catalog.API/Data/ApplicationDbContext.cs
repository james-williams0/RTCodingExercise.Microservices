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
    public DbSet<PlateStatus> PlateStatuses { get; set; }

    public async Task<List<Plate>> GetPlates(
        int pageNumber,
        int pageSize,
        SortBy sortBy = SortBy.Alphabetical,
        OrderBy orderBy = OrderBy.Asc,
        string? searchTerm = null)
    {
        var searchedPlates = string.IsNullOrWhiteSpace(searchTerm)
            ? Plates
            : Plates
                .AsEnumerable()
                .Where(p => _plateFuzzySearcher.IsMatch(p.Registration, searchTerm))
                .AsQueryable();
        
        var orderedPlates = OrderPlatesBy(searchedPlates, sortBy, orderBy);
        
        var pagedPlates = orderedPlates
            .Skip(pageSize * (pageNumber - 1))
            .Take(pageSize);
            
        return string.IsNullOrWhiteSpace(searchTerm)
            ? await pagedPlates.ToListAsync()
            : pagedPlates.ToList();
    }

    public Task<bool> ReservePlate(Guid plateId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SellPlate(Guid plateId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UnreservePlate(Guid plateId)
    {
        throw new NotImplementedException();
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PlateStatus>(entity =>
        {
            entity.HasKey(ps => ps.Id);
            entity.HasOne(ps => ps.Plate)
                .WithOne()
                .HasForeignKey<PlateStatus>(ps => ps.Id)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(ps => ps.Status)
                .IsRequired();
        });
    }
}
