using Catalog.Domain.Enums;

namespace Catalog.Domain.Api.Responses;

public record PlateDto
{
    public Guid Id { get; init; }

    public string? Registration { get; init; }

    public decimal PurchasePrice { get; init; }

    public decimal SalePrice { get; init; }

    public string? Letters { get; init; }

    public int Numbers { get; init; }

    public PlateStatusOption Status { get; init; } = PlateStatusOption.None;
}
