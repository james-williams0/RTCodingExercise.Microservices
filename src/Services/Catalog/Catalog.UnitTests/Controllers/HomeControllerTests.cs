using System;
using Xunit;
using NSubstitute;
using Catalog.API.Controllers;
using Catalog.API.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain;
using Catalog.Domain.Api.Requests;
using Catalog.Domain.Api.Responses;

namespace Catalog.UnitTests.Controllers;

public class HomeControllerTests
{
    [Fact]
    public async Task GetPagedPlates_WhenCalledWithPagedParameters_ThenCallsDbContextWithThoseParameters()
    {
        // Arrange
        var databaseContext = Substitute.For<IApplicationDbContext>();
        var controller = new HomeController(databaseContext);
        var request = new PlatesApiRequest { PageNumber = 2, PageSize = 10 };
        var expectedPlates = new List<Plate> { new() { Id = Guid.NewGuid(), Registration = Guid.NewGuid().ToString() } };
        databaseContext.GetPlates(2, 10).Returns(expectedPlates);

        // Act
        await controller.GetPagedPlates(request);

        // Assert
        await databaseContext.Received(1).GetPlates(2, 10);
    }

    [Fact]
    public async Task GetPagedPlates_WhenCalled_ThenReturnsOkWithPlates()
    {
        // Arrange
        const int pageNumber = 1;
        const int pageSize = 5;
        
        var databaseContext = Substitute.For<IApplicationDbContext>();
        var controller = new HomeController(databaseContext);
        var request = new PlatesApiRequest { PageNumber = pageNumber, PageSize = pageSize };
        var expectedPlates = new List<Plate> { new() { Id = Guid.NewGuid(), Registration = Guid.NewGuid().ToString() } };
        
        databaseContext.GetPlates(pageNumber, pageSize).Returns(expectedPlates);
        databaseContext.GetTotalCount().Returns(expectedPlates.Count);
        
        var expectedResponse = new PlatesApiResponse
        {
            Plates = expectedPlates.Select(plate => new PlateDto
            {
                Id = plate.Id,
                Registration = plate.Registration
            }).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = expectedPlates.Count
        };

        // Act
        var result = await controller.GetPagedPlates(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedResponse, okResult.Value as PlatesApiResponse);
    }

    [Fact]
    public async Task GetPagedPlates_WhenSearchTermProvided_PassesSearchTermToDbContext()
    {
        // Arrange
        var searchTerm = Guid.NewGuid().ToString();
        
        var databaseContext = Substitute.For<IApplicationDbContext>();
        var controller = new HomeController(databaseContext);
        var request = new PlatesApiRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = searchTerm
        };
        List<Plate> expectedPlates =
        [
            new()
            {
                Id = Guid.NewGuid(),
                Registration = searchTerm
            }
        ];
        databaseContext.GetPlates(1, 10, default, default, searchTerm).Returns(expectedPlates);
        databaseContext.GetTotalCount().Returns(expectedPlates.Count);

        // Act
        await controller.GetPagedPlates(request);

        // Assert
        await databaseContext.Received(1).GetPlates(1, 10, default, default, searchTerm);
    }
    
    [Fact]
    public async Task ReservePlate_WhenRequested_ThenCallsDbContextWithPlateId()
    {
        // Arrange
        var plateId = Guid.NewGuid();
        var databaseContext = Substitute.For<IApplicationDbContext>();
        var controller = new HomeController(databaseContext);

        // Act
        await controller.ReservePlate(plateId);

        // Assert
        await databaseContext.Received(1).ReservePlate(plateId);
    }
    
    [Fact]
    public async Task SellPlate_WhenRequested_ThenCallsDbContextWithPlateId()
    {
        // Arrange
        var plateId = Guid.NewGuid();
        var databaseContext = Substitute.For<IApplicationDbContext>();
        var controller = new HomeController(databaseContext);

        // Act
        await controller.SellPlate(plateId);

        // Assert
        await databaseContext.Received(1).ReservePlate(plateId);
    }
    
    [Fact]
    public async Task UnreservePlate_WhenRequested_ThenCallsDbContextWithPlateId()
    {
        // Arrange
        var plateId = Guid.NewGuid();
        var databaseContext = Substitute.For<IApplicationDbContext>();
        var controller = new HomeController(databaseContext);

        // Act
        await controller.UnreservePlate(plateId);

        // Assert
        await databaseContext.Received(1).ReservePlate(plateId);
    }
}
