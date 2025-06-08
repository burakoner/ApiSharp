namespace ApiSharp.Extensions;

/// <summary>
/// String extensions for manipulating and formatting strings
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Join a collection of strings with a specified separator.
    /// </summary>
    /// <param name="values"></param>
    /// <param name="seperator"></param>
    /// <returns></returns>
    public static string Join(this IEnumerable<string> values, string seperator)
    {
        if (values == null || !values.Any()) 
            return string.Empty;

        return string.Join(seperator, values);
    }

    /// <summary>
    /// Is the string a valid numeric value?
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool IsNumeric(this string text) => double.TryParse(text, out _);

}
