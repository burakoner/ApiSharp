namespace ApiSharp.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class LabelAttribute : Attribute
{
    public static readonly LabelAttribute Default = new LabelAttribute();

    public LabelAttribute() : this(string.Empty)
    {
    }

    public LabelAttribute(string label)
    {
        this.Label = label;
    }

    public string Label { get; set; }

    public override bool Equals(object obj)
    {
        if (obj == this)
            return true;

        LabelAttribute other = obj as LabelAttribute;
        return (other != null) && other.Label == this.Label;
    }

    public override int GetHashCode()
    {
        return Label.GetHashCode();
    }

    public override bool IsDefaultAttribute()
    {
        return (Equals(Default));
    }
}

public static class LabelExtensions
{
    public static string GetLabel(this Enum value)
    {
        var type = value.GetType();
        string name = Enum.GetName(type, value);
        if (name != null)
        {
            var field = type.GetField(name);
            if (field != null)
            {
                // var attr = Attribute.GetCustomAttribute(field, typeof(LabelAttribute)) as LabelAttribute;
                // if (attr != null)  return attr.Label;
                var attributes = Attribute.GetCustomAttributes(field);
                if (attributes != null)
                {
                    foreach (var attribute in attributes)
                    {
                        if (attribute.GetType().Name == typeof(LabelAttribute).Name)
                        {
                            // Serialize
                            var json = JsonConvert.SerializeObject(attribute);
                            var label = JsonConvert.DeserializeObject<LabelAttribute>(json);
                            return label.Label;
                        }
                    }
                }
            }
        }

        return string.Empty;
    }

    public static T GetEnumByLabel<T>(this string @this) where T : Enum
    {
        // Get Default Value
        var defaultValue = default(T);

        // Check Point
        if (string.IsNullOrEmpty(@this))
        {
            return defaultValue;
        }

        // Action
        foreach (T item in Enum.GetValues(typeof(T)))
        {
            if (@this.Trim().Equals(item.GetLabel(), StringComparison.OrdinalIgnoreCase))
                return item;
        }

        // Return Dummy
        return defaultValue;
    }

    public static T GetEnumByValue<T>(this int @this) where T : Enum
    {
        // Get Default Value
        var defaultValue = default(T);

        // Action
        foreach (T item in Enum.GetValues(typeof(T)))
        {
            Enum test = Enum.Parse(typeof(T), item.ToString()) as Enum;
            int intValue = Convert.ToInt32(test);

            if (@this == intValue)
                return item;
        }

        // Return Dummy
        return defaultValue;
    }

    public static int GetValue<T>(this T @this) where T : Enum
    {
        return Convert.ToInt32(@this);
    }

    public static int GetValueByLabel<T>(this string @this) where T : Enum
    {
        return Convert.ToInt32(@this.GetEnumByLabel<T>());
    }

    public static string GetLabelByValue<T>(this int @this) where T : Enum
    {
        return @this.GetEnumByValue<T>().GetLabel();
    }

}
