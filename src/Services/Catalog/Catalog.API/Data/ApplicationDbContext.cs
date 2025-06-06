using Catalog.API.Services;
using Catalog.Domain.Api.Requests.Enums;
using Catalog.Domain.Enums;

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

    public Task<List<PlateStatus>> GetPlateStatuses(List<Guid> plateIds)
    {
        return PlateStatuses
            .Where(plateStatuses => plateIds.Contains(plateStatuses.Id))
            .ToListAsync();
    }

    public async Task<bool> ReservePlate(Guid plateId)
    {
        var (plate, plateStatus) = GetPlateAndStatus(plateId);
        
        if (plateStatus is { Status: PlateStatusOption.Sold })
        {
            return false;
        }

        if (plateStatus == null)
        {
            await PlateStatuses.AddAsync(
                new PlateStatus
                {
                    Id = plateId,
                    Status = PlateStatusOption.Reserved,
                    Plate = plate
                });
        }

        await SaveChangesAsync();
        return true;
    }

    public async Task<bool> SellPlate(Guid plateId)
    {
        var (plate, plateStatus) = GetPlateAndStatus(plateId);
        
        if (plateStatus == null)
        {
            await PlateStatuses.AddAsync(
                new PlateStatus
                {
                    Id = plateId,
                    Status = PlateStatusOption.Sold,
                    Plate = plate
                });
        }
        else if (plateStatus.Status != PlateStatusOption.Sold)
        {
            plateStatus.Status = PlateStatusOption.Sold;
            PlateStatuses.Update(plateStatus);
        }

        await SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnreservePlate(Guid plateId)
    {
        var (plate, plateStatus) = GetPlateAndStatus(plateId);
        
        if (plateStatus == null)
        {
            await PlateStatuses.AddAsync(
                new PlateStatus
                {
                    Id = plateId,
                    Status = PlateStatusOption.Sold,
                    Plate = plate
                });
        }
        else if (plateStatus.Status == PlateStatusOption.Reserved)
        {
            PlateStatuses.Remove(plateStatus);
        }

        await SaveChangesAsync();
        return true;
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
    
    private (Plate Plate, PlateStatus? PlateStatus) GetPlateAndStatus(Guid plateId)
    {
        var plate = Plates.Find(plateId);
        if (plate == null)
        {
            throw new InvalidOperationException($"Plate with id {plateId} does not exist.");
        }

        var plateStatus = PlateStatuses.Find(plateId);
        return (plate, plateStatus);
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
