namespace ApiSharp.Attributes;

public class MapAttribute : Attribute
{
    public string[] Values { get; set; }

    public MapAttribute(params string[] maps)
    {
        Values = maps;
    }
}
