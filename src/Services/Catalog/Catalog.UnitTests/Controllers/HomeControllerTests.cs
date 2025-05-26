using System;
using Xunit;
using NSubstitute;
using Catalog.API.Controllers;
using Catalog.API.Data;
using Catalog.API.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Catalog.Domain;

namespace Catalog.UnitTests.Controllers;

public class HomeControllerTests
{
    [Fact]
    public void GetPagedPlates_WhenCalledWithPagedParameters_ThenCallsDbContextWithThoseParameters()
    {
        // Arrange
        var databaseContext = Substitute.For<IApplicationDbContext>();
        var controller = new HomeController(databaseContext);
        var request = new PlatesApiRequest { PageNumber = 2, PageSize = 10 };
        var expectedPlates = new List<Plate> { new() { Id = Guid.NewGuid(), Registration = Guid.NewGuid().ToString() } };
        databaseContext.GetPlates(2, 10).Returns(expectedPlates);

        // Act
        var result = controller.GetPagedPlates(request);

        // Assert
        databaseContext.Received(1).GetPlates(2, 10);
    }

    [Fact]
    public void GetPagedPlates_WhenCalled_ThenReturnsOkWithPlates()
    {
        // Arrange
        var databaseContext = Substitute.For<IApplicationDbContext>();
        var controller = new HomeController(databaseContext);
        var request = new PlatesApiRequest { PageNumber = 1, PageSize = 5 };
        var expectedPlates = new List<Plate> { new() { Id = Guid.NewGuid(), Registration = Guid.NewGuid().ToString() } };
        databaseContext.GetPlates(1, 5).Returns(expectedPlates);

        // Act
        var result = controller.GetPagedPlates(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedPlates, okResult.Value);
    }
}
