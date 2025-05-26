namespace Catalog.API.Data;

public interface IApplicationDbContext
{
    List<Plate> GetPlates(int pageNumber, int pageSize);
}
