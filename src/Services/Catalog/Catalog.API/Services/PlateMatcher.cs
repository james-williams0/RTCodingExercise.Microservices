using System.Text;
using System.Text.RegularExpressions;

namespace Catalog.API.Services;

public interface IPlateFuzzySearchService
{
    bool IsMatch(string registration, string searchTerm);
}

public class PlateFuzzySearchService : IPlateFuzzySearchService
{
    private readonly Dictionary<char, char[]> _characterSubstitutions = new()
    {
        {'A', ['4', 'A'] },
        {'B', ['8', 'B'] },
        {'E', ['3', 'E'] },
        {'G', ['6', 'G'] },
        {'I', ['1', 'I'] },
        {'O', ['0', 'O'] },
        {'S', ['5', 'S', '2'] },
        {'T', ['7', 'T'] },
        {'Z', ['2', 'Z'] },
    };

    public bool IsMatch(string registration, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(registration) || string.IsNullOrWhiteSpace(searchTerm))
            return false;

        // Normalize strings
        var normalizedReg = registration.Replace(" ", "").ToUpperInvariant();
        var normalizedSearch = searchTerm.Replace(" ", "").ToUpperInvariant();
    
        // Build a pattern that allows characters between each letter
        var pattern = new StringBuilder();
        pattern.Append(".*"); // Allow any characters at the start
    
        foreach (var c in normalizedSearch)
        {
            // Add character with substitutions
            if (_characterSubstitutions.TryGetValue(c, out var substitutes))
            {
                pattern.Append('[');
                foreach (var sub in substitutes)
                {
                    pattern.Append(sub);
                }
                pattern.Append(']');
            }
            else
            {
                pattern.Append(c);
            }
        
            pattern.Append(".*"); // Allow any characters between letters
        }
    
        return Regex.IsMatch(normalizedReg, pattern.ToString());
    }
    
    private string BuildSearchPattern(string searchTerm)
    {
        var pattern = new StringBuilder();
        foreach (var c in searchTerm)
        {
            if (_characterSubstitutions.TryGetValue(c, out var substitutes))
            {
                pattern.Append('[');
                foreach (var sub in substitutes)
                {
                    pattern.Append(sub);
                }
                pattern.Append(']');
            }
            else
            {
                pattern.Append(c);
            }
        }
        return pattern.ToString();
    }
}