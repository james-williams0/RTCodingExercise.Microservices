using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Catalog.Domain;
using Catalog.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.UnitTests;

public class ApplicationDbContextTests
{
    private static List<Plate> GenerateRandomPlates(int count)
    {
        var rnd = new Random(418);
        var plates = new List<Plate>();
        for (var i = 0; i < count; i++)
        {
            var letters = Guid.NewGuid().ToString();
            var numbers = rnd.Next(1000, 9999);
            plates.Add(new Plate
            {
                Id = Guid.NewGuid(),
                Registration = $"{letters}{numbers}",
                PurchasePrice = rnd.Next(1, 1000),
                SalePrice = rnd.Next(1, 1000),
                Letters = letters,
                Numbers = numbers
            });
        }
        return plates;
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 5)]
    public void GetPlates_WhenPaged_ThenReturnsExpectedPage(int pageNumber, int pageSize)
    {
        // Arrange
        var plates = GenerateRandomPlates(30);
        var database = Guid.NewGuid().ToString();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        using var dbContext = new ApplicationDbContext(options);
        dbContext.Plates.AddRange(plates);
        dbContext.SaveChanges();

        // Act
        var result = dbContext.GetPlates(pageNumber, pageSize);

        // Assert
        Assert.Equal(pageSize, result.Count);
        var expectedPlateIds = plates.OrderBy(p => p.Registration).Skip(pageSize * (pageNumber - 1)).Take(pageSize).Select(p => p.Id);
        var actualPlatesIds = result.Select(p => p.Id);
        Assert.Equal(expectedPlateIds, actualPlatesIds);
    }
}

