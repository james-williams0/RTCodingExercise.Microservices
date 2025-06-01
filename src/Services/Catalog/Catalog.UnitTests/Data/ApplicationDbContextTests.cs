using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.API.Data;
using Catalog.API.Services;
using Catalog.Domain;
using Catalog.Domain.Api.Requests.Enums;
using Catalog.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace Catalog.UnitTests.Data;

public class ApplicationDbContextTests
{
    private static List<Plate> GenerateRandomPlates(int count)
    {
        var random = new Random(418);
        var plates = new List<Plate>();
        for (var i = 0; i < count; i++)
        {
            var letters = Guid.NewGuid().ToString();
            var numbers = random.Next(1000, 9999);
            plates.Add(new Plate
            {
                Id = Guid.NewGuid(),
                Registration = $"{letters}{numbers}",
                PurchasePrice = random.Next(1, 1000),
                SalePrice = random.Next(1, 1000),
                Letters = letters,
                Numbers = numbers
            });
        }
        return plates;
    }

    [Theory]
    [InlineData(1, 10, SortBy.Alphabetical, OrderBy.Asc)]
    [InlineData(2, 5, SortBy.Alphabetical, OrderBy.Asc)]
    [InlineData(1, 10, SortBy.Price, OrderBy.Asc)]
    [InlineData(2, 5, SortBy.Price, OrderBy.Asc)]
    [InlineData(1, 10, SortBy.Alphabetical, OrderBy.Desc)]
    [InlineData(2, 5, SortBy.Alphabetical, OrderBy.Desc)]
    [InlineData(1, 10, SortBy.Price, OrderBy.Desc)]
    [InlineData(2, 5, SortBy.Price, OrderBy.Desc)]
    public async Task GetPlates_WhenPaged_ThenReturnsExpectedPage(
        int pageNumber,
        int pageSize,
        SortBy sortBy,
        OrderBy orderBy)
    {
        // Arrange
        var plates = GenerateRandomPlates(30);
        var database = Guid.NewGuid().ToString();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        await using var dbContext = new ApplicationDbContext(options, new PlateFuzzySearcher());
        await dbContext.Plates.AddRangeAsync(plates);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.GetPlates(pageNumber, pageSize, sortBy, orderBy);

        // Assert
        Assert.Equal(pageSize, result.Count);

        var orderedPlates = (sortBy, orderBy) switch
        {
            (SortBy.Alphabetical, OrderBy.Asc) => plates.OrderBy(p => p.Registration),
            (SortBy.Price, OrderBy.Asc) => plates.OrderBy(p => p.SalePrice),
            (SortBy.Alphabetical, OrderBy.Desc) => plates.OrderByDescending(p => p.Registration),
            (SortBy.Price, OrderBy.Desc) => plates.OrderByDescending(p => p.SalePrice),
            _ => throw new InvalidOperationException("Sort/order combination untested")
        };
        
        var expectedPlateIds = orderedPlates.Skip(pageSize * (pageNumber - 1)).Take(pageSize).Select(p => p.Id);
        var actualPlatesIds = result.Select(p => p.Id);
        Assert.Equal(expectedPlateIds, actualPlatesIds);
    }

    [Fact]
    public async Task GetTotalCount_WhenCalled_ThenReturnsExpectedCount()
    {
        // Arrange
        const int expectedCount = 15;
        
        var plates = GenerateRandomPlates(expectedCount);
        var database = Guid.NewGuid().ToString();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        await using var dbContext = new ApplicationDbContext(options, new PlateFuzzySearcher());
        await dbContext.Plates.AddRangeAsync(plates);
        await dbContext.SaveChangesAsync();

        // Act
        var actualCount = await dbContext.GetTotalCount();

        // Assert
        Assert.Equal(expectedCount, actualCount);
    }

    [Fact]
    public async Task GetPlates_WhenSearched_ThenReturnsMatchingPlates()
    {
        // Arrange
        var searchTerm = Guid.NewGuid().ToString();
        var database = Guid.NewGuid().ToString();
        var plates = GenerateRandomPlates(1);
        
        var searcher = Substitute.For<IPlateFuzzySearcher>();
        searcher.IsMatch(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        await using var dbContext = new ApplicationDbContext(options, searcher);
        await dbContext.Plates.AddRangeAsync(plates);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.GetPlates(1, 10, searchTerm: searchTerm);

        // Assert
        searcher.Received(1).IsMatch(
            Arg.Is<string>(s => s == plates[0].Registration),
            Arg.Is<string>(s => s == searchTerm));
        Assert.Equal(result.Count, plates.Count);
    }
    
    [Fact]
    public async Task GetPlates_WhenSearched_ThenDoesNotReturnsNonMatchingPlates()
    {
        // Arrange
        var searchTerm = Guid.NewGuid().ToString();
        var database = Guid.NewGuid().ToString();
        var plate = GenerateRandomPlates(1).Single();
        
        var searcher = Substitute.For<IPlateFuzzySearcher>();
        searcher.IsMatch(Arg.Any<string>(), Arg.Any<string>()).Returns(false);
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        await using var dbContext = new ApplicationDbContext(options, searcher);
        await dbContext.Plates.AddAsync(plate);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await dbContext.GetPlates(1, 10, searchTerm: searchTerm);

        // Assert
        searcher.Received(1).IsMatch(
            Arg.Is<string>(s => s == plate.Registration),
            Arg.Is<string>(s => s == searchTerm));
        Assert.Empty(result);
    }

    [Fact]
    public async Task GivenAReserveCommand_WhenPlateIsUnreservedAndUnsold_ThenReserveIt()
    {
        // Arrange
        var plate = GenerateRandomPlates(1).Single();
        var database = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        await using var dbContext = new ApplicationDbContext(options, new PlateFuzzySearcher());
        await dbContext.Plates.AddAsync(plate);
        await dbContext.SaveChangesAsync();

        // Act
        await dbContext.ReservePlate(plate.Id);

        // Assert
        var plateStatus = await dbContext.PlateStatuses.SingleAsync(p => p.Id == plate.Id);
        Assert.Equal(PlateStatuses.Reserved, plateStatus.Status);
    }
    
    [Fact]
    public async Task GivenAReserveCommand_WhenPlateIsSold_ThenPlateIsStillSold()
    {
        // Arrange
        var plate = GenerateRandomPlates(1).Single();
        var database = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        await using var dbContext = new ApplicationDbContext(options, new PlateFuzzySearcher());
        await dbContext.Plates.AddAsync(plate);
        await dbContext.SaveChangesAsync();
        await dbContext.SellPlate(plate.Id);

        // Act
        await dbContext.ReservePlate(plate.Id);

        // Assert
        var plateStatus = await dbContext.PlateStatuses.SingleAsync(p => p.Id == plate.Id);
        Assert.Equal(PlateStatuses.Sold, plateStatus.Status);
    }
    
    [Fact]
    public async Task GivenASellCommand_WhenPlateIsUnreservedAndUnsold_ThenPlateIsSold()
    {
        // Arrange
        var plate = GenerateRandomPlates(1).Single();
        var database = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        await using var dbContext = new ApplicationDbContext(options, new PlateFuzzySearcher());
        await dbContext.Plates.AddAsync(plate);
        await dbContext.SaveChangesAsync();

        // Act
        await dbContext.ReservePlate(plate.Id);

        // Assert
        var plateStatus = await dbContext.PlateStatuses.SingleAsync(p => p.Id == plate.Id);
        Assert.Equal(PlateStatuses.Sold, plateStatus.Status);
    }
    
    [Fact]
    public async Task GivenASellCommand_WhenPlateIsReserved_ThenPlateIsSold()
    {
        // Arrange
        var plate = GenerateRandomPlates(1).Single();
        var database = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        await using var dbContext = new ApplicationDbContext(options, new PlateFuzzySearcher());
        await dbContext.Plates.AddAsync(plate);
        await dbContext.SaveChangesAsync();
        await dbContext.SellPlate(plate.Id);
        await dbContext.ReservePlate(plate.Id);

        // Act
        await dbContext.ReservePlate(plate.Id);

        // Assert
        var plateStatus = await dbContext.PlateStatuses.SingleAsync(p => p.Id == plate.Id);
        Assert.Equal(PlateStatuses.Sold, plateStatus.Status);
    }
    
    [Fact]
    public async Task GivenAnUnreserveCommand_WhenPlateIsReserved_ThenUnreserveIt()
    {
        // Arrange
        var plate = GenerateRandomPlates(1).Single();
        var database = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        await using var dbContext = new ApplicationDbContext(options, new PlateFuzzySearcher());
        await dbContext.Plates.AddAsync(plate);
        await dbContext.SaveChangesAsync();
        await dbContext.ReservePlate(plate.Id);

        // Act
        await dbContext.UnreservePlate(plate.Id);

        // Assert
        var plateStatus = await dbContext.PlateStatuses.AnyAsync(p => p.Id == plate.Id);
        Assert.False(plateStatus);
    }
    
    [Fact]
    public async Task GivenAnUnreserveCommand_WhenPlateIsSold_ThenRemainSold()
    {
        // Arrange
        var plate = GenerateRandomPlates(1).Single();
        var database = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(database)
            .Options;
        await using var dbContext = new ApplicationDbContext(options, new PlateFuzzySearcher());
        await dbContext.Plates.AddAsync(plate);
        await dbContext.SaveChangesAsync();
        await dbContext.SellPlate(plate.Id);

        // Act
        await dbContext.UnreservePlate(plate.Id);

        // Assert
        var plateStatus = await dbContext.PlateStatuses.SingleAsync(p => p.Id == plate.Id);
        Assert.Equal(PlateStatuses.Sold, plateStatus.Status);
    }
}
