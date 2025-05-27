using Catalog.API.Services;
using Xunit;

namespace Catalog.UnitTests.Services;

public class PlateFuzzySearcherTests
{
    [Theory]
    [InlineData("Danny", "DA12 NNY")]
    [InlineData("G Smith", "GSM 17H")]
    [InlineData("James", "JAM 3S")]
    public void GivenAPlate_WhenSearchingForRegistration_ThenReturnsTrueIfRegistrationIsSimilar(
        string searchTerm,
        string registration)
    {
        // Arrange
        var searcher = new PlateFuzzySearcher();

        // Act
        var result = searcher.IsMatch(registration, searchTerm);

        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void GivenAPlate_WhenSearchingForRegistration_ThenReturnsFalseIfRegistrationIsNotSimilar()
    {
        // Arrange
        const string registration = "YT10 KUC";
        const string searchTerm = "Dave";
        
        var searcher = new PlateFuzzySearcher();

        // Act
        var result = searcher.IsMatch(registration, searchTerm);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GivenAPlate_WhenSearchingForRegistration_ThenReturnsFalseIfRegistrationIsNull()
    {
        // Arrange
        const string? registration = null;
        const string searchTerm = "Dave";

        var searcher = new PlateFuzzySearcher();

        // Act
        var result = searcher.IsMatch(registration, searchTerm);

        // Assert
        Assert.False(result);
    }
}
