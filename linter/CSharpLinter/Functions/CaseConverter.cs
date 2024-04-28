using System.Globalization;

public class CaseConverter
{
    public static string ToCamelCase(string input)
    {
        var tokens = SplitIntoWords(input);
        return string.Concat(
            tokens.Select(
                (s, i) =>
                    i == 0
                        ? s.ToLower()
                        : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower())
            )
        );
    }

    public static string ToPascalCase(string input)
    {
        var tokens = SplitIntoWords(input);
        return string.Concat(
            tokens.Select(s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower()))
        );
    }

    public static string ToSnakeCase(string input)
    {
        var tokens = SplitIntoWords(input);
        return string.Join("_", tokens.Select(s => s.ToLower()));
    }

    public static string ToKebabCase(string input)
    {
        var tokens = SplitIntoWords(input);
        return string.Join("-", tokens.Select(s => s.ToLower()));
    }

    private static string[] SplitIntoWords(string input)
    {
        var cleanedInput = System.Text.RegularExpressions.Regex.Replace(
            input,
            "[^a-zA-Z0-9_-]",
            ""
        );
        return System.Text.RegularExpressions.Regex.Split(cleanedInput, @"(?<!^)(?=[A-Z])|_|-");
    }
}
