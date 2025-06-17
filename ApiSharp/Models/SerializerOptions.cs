namespace ApiSharp.Models;

/// <summary>
/// Serializer options
/// </summary>
public static class SerializerOptions
{
    /// <summary>
    /// Json serializer settings which includes the EnumConverter, DateTimeConverter and BoolConverter
    /// </summary>
    public static JsonSerializerSettings WithConverters => new JsonSerializerSettings
    {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        Culture = CultureInfo.InvariantCulture,
        Converters =
        {
            new MapConverter(),
            new DateTimeConverter(),
            new BooleanConverter()
        },
    };

    /// <summary>
    /// Default json serializer settings
    /// </summary>
    public static JsonSerializerSettings Default => new JsonSerializerSettings
    {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        Culture = CultureInfo.InvariantCulture
    };
}
