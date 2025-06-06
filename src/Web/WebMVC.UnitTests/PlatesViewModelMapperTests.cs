using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Catalog.Domain;
using Catalog.Domain.Api.Responses;
using Catalog.Domain.Enums;
using RTCodingExercise.Microservices.Services;

namespace WebMVC.UnitTests;

public class PlatesViewModelMapperTests
{
    private static PlatesApiResponse GenerateRandomPlatesApiResponse(
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        var random = new Random(418);
        var plates = new List<PlateDto>();
        for (var i = 0; i < pageSize; i++)
        {
            var letters = Guid.NewGuid().ToString();
            var numbers = random.Next(1000, 9999);
            plates.Add(new PlateDto
            {
                Id = Guid.NewGuid(),
                Registration = $"{letters}{numbers}",
                PurchasePrice = random.Next(1000, 9999),
                SalePrice = random.Next(1000, 9999),
                Letters = letters,
                Numbers = numbers,
                Status = PlateStatusOption.None
            });
        }
        return new PlatesApiResponse
        {
            Plates = plates,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(40, 2, 20)]
    public void GivenAPlatesApiResponse_WhenMapped_ReturnsExpectedPlatesViewModel(
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        // Arrange
        var response = GenerateRandomPlatesApiResponse(
            totalCount,
            pageNumber,
            pageSize);
        var mapper = new PlatesViewModelMapper();
        
        // Act
        var viewModel = mapper.Map(response);
        
        // Assert
        Assert.Equal(response.PageNumber, viewModel.PageNumber);
        Assert.Equal(response.PageSize, viewModel.PageSize);
        Assert.Equal(totalCount, viewModel.TotalCount);
        Assert.Equal(pageSize, viewModel.Plates.Count);
        Assert.Equal(response.Plates.Select(p => p.Registration), viewModel.Plates.Select(p => p.Registration));
    }
}
