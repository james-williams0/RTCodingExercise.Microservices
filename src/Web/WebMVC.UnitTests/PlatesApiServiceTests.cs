using System;
using System.Threading.Tasks;
using Catalog.Domain;
using Catalog.Domain.Api.Responses;
using NSubstitute;
using RTCodingExercise.Microservices.Services;
using WebMVC.Services;
using Xunit;

namespace WebMVC.UnitTests;

public class PlatesApiServiceTests
{
    [Fact]
    public async Task GivenARequestForPlates_WhenApiResponseReceived_ReturnsExpectedViewModel()
    {
        // Arrange
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
            PageNumber = 1,
            PageSize = 2,
            TotalCount = 2
        };
        
        var catalogClient = Substitute.For<ICatalogApi>();
        catalogClient.GetPagedPlatesAsync(1, 2).Returns(platesApiResponse);
        
        var mapper = new PlatesViewModelMapper();
        
        var service = new PlatesApiService(catalogClient, mapper);

        // Act
        var actualResult = await service.GetPagedPlatesAsync(1, 2);
        var expectedResult = mapper.Map(platesApiResponse);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(platesApiResponse.Plates.Count, actualResult.Plates.Count);
            Assert.Equivalent(expectedResult, actualResult);
        });
    }
}
