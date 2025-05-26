namespace Catalog.API.Data;

public interface IApplicationDbContext
{
    Task<List<Plate>> GetPlates(int pageNumber, int pageSize);
    
    Task<int> GetTotalCount();
}
