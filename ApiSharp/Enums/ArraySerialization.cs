namespace ApiSharp.Enums;

/// <summary>
/// Define how array parameters should be send
/// </summary>
public enum ArraySerialization
{
    /// <summary>
    /// Send multiple key=value for each entry
    /// </summary>
    MultipleValues,

    /// <summary>
    /// Create an []=value array
    /// </summary>
    Array
}
