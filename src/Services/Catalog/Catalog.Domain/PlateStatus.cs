using Catalog.Domain.Enums;

namespace Catalog.Domain;

public class PlateStatus
{
    public Guid Id { get; init; }
        
    public PlateStatusOption Status { get; set; }
        
    public required Plate Plate { get; init; }
}
