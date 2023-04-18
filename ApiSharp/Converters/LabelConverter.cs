namespace ApiSharp.Converters;

public class LabelConverter<T> : BaseConverter<T> where T : struct
{
    public LabelConverter() : this(true) { }
    public LabelConverter(bool quotes) : base(quotes) { }

    protected override List<KeyValuePair<T, string>> Mapping
    {
        get
        {
            var kvp = new List<KeyValuePair<T, string>>();
            foreach (T val in Enum.GetValues(typeof(T)))
            {
                kvp.Add(new KeyValuePair<T, string>(val, (val as Enum).GetLabel()));
            }
            return kvp;
        }
    }
}