namespace ApiSharp.Attributes;

/// <summary>
/// Map a enum entry to string values
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class MapAttribute(params string[] maps) : Attribute
{
    /// <summary>
    /// Values mapping to the enum entry
    /// </summary>
    public string[] Values { get; set; } = maps;
}
