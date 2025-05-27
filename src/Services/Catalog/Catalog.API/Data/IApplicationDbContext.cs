using Catalog.Domain.Api.Requests.Enums;

namespace Catalog.API.Data;

public interface IApplicationDbContext
{
    Task<List<Plate>> GetPlates(int pageNumber, int pageSize, SortBy sortBy, OrderBy orderBy);
    
    Task<int> GetTotalCount();
}
