using System;
using System.Threading.Tasks;
using Catalog.Domain;
using Catalog.Domain.Api.Requests;
using Catalog.Domain.Api.Requests.Enums;
using Catalog.Domain.Api.Responses;
using NSubstitute;
using RTCodingExercise.Microservices.Services;
using WebMVC.Services;
using Xunit;

namespace WebMVC.UnitTests;

public class PlatesApiServiceTests
{
    [Fact]
    public async Task GivenARequestForPlates_WhenApiResponseReceived_ThenMappedApiResponseIsReturned()
    {
        // Arrange
        const int pageNumber = 1;
        const int pageSize = 2;
        
        var random = new Random(418);
        var platesApiResponse = new PlatesApiResponse
        {
            Plates =
            [
                new Plate
                {
                    Registration = Guid.NewGuid().ToString(),
                    PurchasePrice = random.Next(1000, 9000),
                    SalePrice = random.Next(1000, 9000)
                },
                new Plate
                {
                    Registration = Guid.NewGuid().ToString(),
                    PurchasePrice = random.Next(1000, 9000),
                    SalePrice = random.Next(1000, 9000)
                }
            ],
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = pageSize
        };
        
        var catalogClient = Substitute.For<ICatalogApi>();
        catalogClient
            .GetPagedPlatesAsync(Arg.Any<PlatesApiRequest>())
            .Returns(platesApiResponse);
        
        var mapper = new PlatesViewModelMapper();
        
        var service = new PlatesApiService(catalogClient, mapper);

        // Act
        var actualResult = await service.GetPagedPlatesAsync(pageNumber, pageSize);
        var expectedResult = mapper.Map(platesApiResponse);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(platesApiResponse.Plates.Count, actualResult.Plates.Count);
            Assert.Equivalent(expectedResult, actualResult);
        });
    }

    [Fact]
    public async Task GivenARequestForPlates_WhenSortedAndOrderedByPriceDescending_ThenPassesSortByAndOrderByToApiRequest()
    {
        // Arrange
        const int pageNumber = 1;
        const int pageSize = 10;
        
        var platesApiResponse = new PlatesApiResponse
        {
            Plates = [],
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 0
        };
        
        var catalogClient = Substitute.For<ICatalogApi>();
        catalogClient
            .GetPagedPlatesAsync(Arg.Any<PlatesApiRequest>())
            .Returns(platesApiResponse);
        
        var service = new PlatesApiService(catalogClient, new PlatesViewModelMapper());

        // Act
        await service.GetPagedPlatesAsync(pageNumber, pageSize);

        // Assert
        await catalogClient.Received(1).GetPagedPlatesAsync(Arg.Is<PlatesApiRequest>(p =>
            p.SortBy == SortBy.Price &&
            p.OrderBy == OrderBy.Desc));
    }
    
    [Fact]
    public async Task GivenARequestForPlates_WhenApiResponseIsReceived_ThenTheResponseIsPassedToTheMapper()
    {
        // Arrange
        const int pageNumber = 1;
        const int pageSize = 10;
        
        var expectedPlatesApiResponse = new PlatesApiResponse
        {
            Plates = [],
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 0
        };
        
        var catalogClient = Substitute.For<ICatalogApi>();
        catalogClient
            .GetPagedPlatesAsync(Arg.Any<PlatesApiRequest>())
            .Returns(expectedPlatesApiResponse);

        PlatesApiResponse actualPlatesApiResponse = null!;
        var mapper = Substitute.For<IPlateViewModelMapper>();
        mapper
            .Map(Arg.Do<PlatesApiResponse>(p => actualPlatesApiResponse = p));
        var service = new PlatesApiService(catalogClient, mapper);

        // Act
        var result = await service.GetPagedPlatesAsync(pageNumber, pageSize);

        // Assert
        mapper.ReceivedWithAnyArgs(1).Map(Arg.Any<PlatesApiResponse>());
        Assert.Equal(expectedPlatesApiResponse, actualPlatesApiResponse);
    }
}
