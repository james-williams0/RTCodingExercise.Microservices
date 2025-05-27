using Catalog.Domain.Api.Requests.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Domain.Api.Requests;

public record PlatesApiRequest
{
    [FromQuery]
    public string? SearchTerm { get; init; }
    
    [FromQuery]
    public int PageNumber { get; init; } = 1;
    
    [FromQuery]
    public int PageSize { get; init; } = 20;
    
    [FromQuery]
    public SortBy SortBy { get; init; } = SortBy.Alphabetical;
    
    [FromQuery]
    public OrderBy OrderBy { get; init; } = OrderBy.Asc;
}
