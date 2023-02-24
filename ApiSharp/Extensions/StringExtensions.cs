namespace ApiSharp.Extensions;

public static class StringExtensions
{
    public static string Join(this IEnumerable<string> values, string seperator)
    {
        if (values == null || !values.Any()) 
            return string.Empty;

        return string.Join(seperator, values);
    }

    public static bool IsNumeric(this string text) => double.TryParse(text, out _);

}
