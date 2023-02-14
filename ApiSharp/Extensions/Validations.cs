namespace ApiSharp.Extensions;

public static class Validations
{
    /// <summary>
    /// Validates a string is not null or empty
    /// </summary>
    /// <param name="value">The value of the string</param>
    /// <param name="argumentName">Name of the parameter</param>
    public static void ValidateNotNull(this string value, string argumentName)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException($"No value provided for parameter {argumentName}", argumentName);
    }

    /// <summary>
    /// Validates an object is not null
    /// </summary>
    /// <param name="value">The value of the object</param>
    /// <param name="argumentName">Name of the parameter</param>
    public static void ValidateNotNull(this object value, string argumentName)
    {
        if (value == null)
            throw new ArgumentException($"No value provided for parameter {argumentName}", argumentName);
    }

    /// <summary>
    /// Validates a list is not null or empty
    /// </summary>
    /// <param name="value">The value of the object</param>
    /// <param name="argumentName">Name of the parameter</param>
    public static void ValidateNotNull<T>(this IEnumerable<T> value, string argumentName)
    {
        if (value == null || !value.Any())
            throw new ArgumentException($"No values provided for parameter {argumentName}", argumentName);
    }

    /// <summary>
    /// Validates an int is one of the allowed values
    /// </summary>
    /// <param name="value">Value of the int</param>
    /// <param name="argumentName">Name of the parameter</param>
    /// <param name="allowedValues">Allowed values</param>
    public static void ValidateIntValues(this int value, string argumentName, params int[] allowedValues)
    {
        if (!allowedValues.Contains(value))
            throw new ArgumentException(
                $"{value} not allowed for parameter {argumentName}, allowed values: {string.Join(", ", allowedValues)}", argumentName);
    }

    /// <summary>
    /// Validates an int is between two values
    /// </summary>
    /// <param name="value">The value of the int</param>
    /// <param name="argumentName">Name of the parameter</param>
    /// <param name="minValue">Min value</param>
    /// <param name="maxValue">Max value</param>
    public static void ValidateIntBetween(this int value, string argumentName, int minValue, int maxValue)
    {
        if (value < minValue || value > maxValue)
            throw new ArgumentException(
                $"{value} not allowed for parameter {argumentName}, min: {minValue}, max: {maxValue}", argumentName);
    }

    /// <summary>
    /// Validates an double is between two values
    /// </summary>
    /// <param name="value">The value of the double</param>
    /// <param name="argumentName">Name of the parameter</param>
    /// <param name="minValue">Min value</param>
    /// <param name="maxValue">Max value</param>
    public static void ValidateDoubleBetween(this double value, string argumentName, double minValue, double maxValue)
    {
        if (value < minValue || value > maxValue)
            throw new ArgumentException(
                $"{value} not allowed for parameter {argumentName}, min: {minValue}, max: {maxValue}", argumentName);
    }

    /// <summary>
    /// Validates an decimal is between two values
    /// </summary>
    /// <param name="value">The value of the decimal</param>
    /// <param name="argumentName">Name of the parameter</param>
    /// <param name="minValue">Min value</param>
    /// <param name="maxValue">Max value</param>
    public static void ValidateDecimalBetween(this decimal value, string argumentName, decimal minValue, decimal maxValue)
    {
        if (value < minValue || value > maxValue)
            throw new ArgumentException(
                $"{value} not allowed for parameter {argumentName}, min: {minValue}, max: {maxValue}", argumentName);
    }
}
