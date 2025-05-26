namespace Catalog.Domain.Api.Responses;

public record PlatesApiResponse
{
    public required List<Plate> Plates { get; init; }
    
    public required int PageNumber { get; init; }
    
    public required int PageSize { get; init; }
    
    public required int TotalCount { get; init; }
    
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
