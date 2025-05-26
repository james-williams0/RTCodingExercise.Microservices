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
}