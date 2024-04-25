using System.Text.RegularExpressions;

public static class StringUtils
{
    private static Regex s_capitalSplitRegex = new Regex(@"
        (?<=[A-Z])(?=[A-Z][a-z]) |
        (?<=[^A-Z])(?=[A-Z]) |
        (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

    public static string SplitCapitals(string text)
    {
        return s_capitalSplitRegex.Replace(text, " ");
    }
}
