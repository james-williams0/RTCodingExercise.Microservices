using Catalog.Domain.Api.Requests.Enums;

namespace Catalog.API.Data;

public interface IApplicationDbContext
{
    Task<List<Plate>> GetPlates(
        int pageNumber,
        int pageSize,
        SortBy sortBy = SortBy.Alphabetical,
        OrderBy orderBy = OrderBy.Asc,
        string? searchTerm = null);
    
    Task<List<PlateStatus>> GetPlateStatuses(List<Guid> plateIds);
    
    Task<bool> ReservePlate(Guid plateId);
    
    Task<bool> SellPlate(Guid plateId);
    
    Task<bool> UnreservePlate(Guid plateId);
    
    Task<int> GetTotalCount();
}
