using Catalog.Domain.Api.Requests;
using Catalog.Domain.Api.Requests.Enums;
using Catalog.Domain.Api.Responses;
using Refit;
using RTCodingExercise.Microservices.Models;
using RTCodingExercise.Microservices.Services;

namespace WebMVC.Services;

public interface ICatalogApi
{
    [Get("/plates")]
    Task<PlatesApiResponse> GetPagedPlatesAsync([Query] PlatesApiRequest request);
}

public class PlatesApiService
{
    private readonly ICatalogApi _catalogApi;
    private readonly IPlateViewModelMapper _plateViewModelMapper;

    public PlatesApiService(ICatalogApi catalogApi, IPlateViewModelMapper plateViewModelMapper)
    {
        _catalogApi = catalogApi;
        _plateViewModelMapper = plateViewModelMapper;
    }

    public async Task<PlatesViewModel> GetPagedPlatesAsync(int pageNumber, int pageSize, string sortBy = "Alphabetical", string orderBy = "Asc")
    {
        var request = new PlatesApiRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = Enum.Parse<SortBy>(sortBy, true),
            OrderBy = Enum.Parse<OrderBy>(orderBy, true)
        };
        var pagedPlates = await _catalogApi.GetPagedPlatesAsync(request);
        var platesViewModel = _plateViewModelMapper.Map(pagedPlates);
        return platesViewModel with { SortBy = sortBy, OrderBy = orderBy };
    }
}
