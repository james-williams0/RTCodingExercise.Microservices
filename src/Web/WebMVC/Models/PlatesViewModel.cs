namespace RTCodingExercise.Microservices.Models;

public record PlateViewModel
{
    private const decimal Markup = 0.2m;
    
    public string? Registration { get; init; }

    public decimal PurchasePrice { get; init; }

    public decimal SalePrice { private get; init; }
    
    public decimal DisplayPrice => SalePrice * (1.0m + Markup);
}

public record PlatesViewModel
{
    public required List<PlateViewModel> Plates { get; init; }
    
    public required int PageNumber { get; init; }
    
    public required int PageSize { get; init; }
    
    public required int TotalCount { get; init; }
    
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    public string SortBy { get; init; } = "Alphabetical";
    
    public string OrderBy { get; init; } = "Asc";

    public string? SearchTerm { get; init; }
}
