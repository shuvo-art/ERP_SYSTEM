using System.Text.RegularExpressions;

namespace ProductApi.Core.Helpers;

public static class SlugHelper
{
    public static string Generate(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Convert to lowercase
        var str = text.ToLowerInvariant();

        // Remove invalid characters
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

        // Convert multiple spaces/hyphens into one hyphen
        str = Regex.Replace(str, @"[\s-]+", "-").Trim('-');

        return str;
    }
}
