using System.Text;
using System.Text.RegularExpressions;

namespace Catalog.API.Services;

public interface IPlateFuzzySearcher
{
    bool IsMatch(string? registration, string searchTerm);
}

public class PlateFuzzySearcher : IPlateFuzzySearcher
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

    public bool IsMatch(string? registration, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || string.IsNullOrWhiteSpace(registration))
        {
            return false;
        }

        var searchableReg = registration.Replace(" ", "").ToUpperInvariant();
        searchTerm = searchTerm.Replace(" ", "").ToUpperInvariant();
        
        var patternBuilder = new StringBuilder();
        patternBuilder.Append(".*");
    
        foreach (var character in searchTerm)
        {
            if (_characterSubstitutions.TryGetValue(character, out var substitutes))
            {
                patternBuilder.Append('[');
                foreach (var substitute in substitutes)
                {
                    patternBuilder.Append(substitute);
                }
                patternBuilder.Append(']');
            }
            else
            {
                patternBuilder.Append(character);
            }
        
            patternBuilder.Append(".*");
        }
    
        return Regex.IsMatch(searchableReg, patternBuilder.ToString());
    }
}
