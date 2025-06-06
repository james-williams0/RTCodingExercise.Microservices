using Catalog.Domain.Api.Responses;
using RTCodingExercise.Microservices.Models;

namespace RTCodingExercise.Microservices.Services;

public interface IPlateViewModelMapper
{
    PlatesViewModel Map(PlatesApiResponse plates);
}

public class PlatesViewModelMapper : IPlateViewModelMapper
{
    private static PlateViewModel Map(PlateDto plate)
    {
        return new PlateViewModel
        {
            Registration = plate.Registration,
            PurchasePrice = plate.PurchasePrice,
            SalePrice = plate.SalePrice
        };
    }

    public PlatesViewModel Map(PlatesApiResponse platesApiResponse)
    {
        return new PlatesViewModel
        {
            Plates = platesApiResponse.Plates.Select(Map).ToList(),
            PageNumber = platesApiResponse.PageNumber,
            PageSize = platesApiResponse.PageSize,
            TotalCount = platesApiResponse.TotalCount
        };
    }
}
