using Catalog.Domain.Api.Requests;
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

    public async Task<PlatesViewModel> GetPagedPlatesAsync(int pageNumber, int pageSize)
    {
        var pagedPlates = await _catalogApi.GetPagedPlatesAsync(new PlatesApiRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        var platesViewModel = _plateViewModelMapper.Map(pagedPlates);
        return platesViewModel;
    }
}
