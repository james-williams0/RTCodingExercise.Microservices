using Catalog.Domain.Enums;

namespace Catalog.Domain;

public class PlateStatus
{
    public Guid Id { get; init; }
        
    public PlateStatuses Status { get; init; }
        
    public required Plate Plate { get; init; }
}
