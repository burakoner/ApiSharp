namespace ApiSharp.Converters;

public class DateTimeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.Value == null)
            return null;

        if (reader.TokenType is JsonToken.Integer)
        {
            var longValue = (long)reader.Value;
            if (longValue == 0 || longValue == -1)
                return objectType == typeof(DateTime) ? default(DateTime) : null;
            if (longValue < 19999999999)
                return longValue.ConvertFromSeconds();
            if (longValue < 19999999999999)
                return longValue.ConvertFromMilliseconds();
            if (longValue < 19999999999999999)
                return longValue.ConvertFromMicroseconds();

            return longValue.ConvertFromNanoseconds();
        }
        else if (reader.TokenType is JsonToken.Float)
        {
            var doubleValue = (double)reader.Value;
            if (doubleValue == 0 || doubleValue == -1)
                return objectType == typeof(DateTime) ? default(DateTime) : null;

            if (doubleValue < 19999999999)
                return doubleValue.ConvertFromSeconds();

            return doubleValue.ConvertFromMilliseconds();
        }
        else if (reader.TokenType is JsonToken.String)
        {
            var stringValue = (string)reader.Value;
            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            if (string.IsNullOrWhiteSpace(stringValue) || stringValue == "0" || stringValue == "-1")
                return objectType == typeof(DateTime) ? default(DateTime) : null;

            if (stringValue.Length == 8)
            {
                // Parse 20211103 format
                if (!int.TryParse(stringValue.Substring(0, 4), out var year)
                    || !int.TryParse(stringValue.Substring(4, 2), out var month)
                    || !int.TryParse(stringValue.Substring(6, 2), out var day))
                {
                    Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Unknown DateTime format: " + reader.Value);
                    return default;
                }
                return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            }

            if (stringValue.Length == 6)
            {
                // Parse 211103 format
                if (!int.TryParse(stringValue.Substring(0, 2), out var year)
                    || !int.TryParse(stringValue.Substring(2, 2), out var month)
                    || !int.TryParse(stringValue.Substring(4, 2), out var day))
                {
                    Trace.WriteLine("{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Unknown DateTime format: " + reader.Value);
                    return default;
                }
                return new DateTime(year + 2000, month, day, 0, 0, 0, DateTimeKind.Utc);
            }

            if (double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
            {
                // Parse 1637745563.000 format
                if (doubleValue < 19999999999)
                    return doubleValue.ConvertFromSeconds();
                if (doubleValue < 19999999999999)
                    return ((long)doubleValue).ConvertFromMilliseconds();
                if (doubleValue < 19999999999999999)
                    return ((long)doubleValue).ConvertFromMicroseconds();

                return ((long)doubleValue).ConvertFromNanoseconds();
            }

            if (stringValue.Length == 10)
            {
                // Parse 2021-11-03 format
                var values = stringValue.Split('-');
                if (!int.TryParse(values[0], out var year)
                    || !int.TryParse(values[1], out var month)
                    || !int.TryParse(values[2], out var day))
                {
                    Trace.WriteLine("{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Unknown DateTime format: " + reader.Value);
                    return default;
                }

                return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            }

            return DateTime.Parse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }
        else if (reader.TokenType == JsonToken.Date)
        {
            return (DateTime)reader.Value;
        }
        else
        {
            Trace.WriteLine("{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Unknown DateTime format: " + reader.Value);
            return default;
        }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var datetimeValue = (DateTime?)value;
        if (datetimeValue == null)
            writer.WriteValue((DateTime?)null);
        if (datetimeValue == default(DateTime))
            writer.WriteValue((DateTime?)null);
        else
            writer.WriteValue((long)Math.Round(((DateTime)value! - new DateTime(1970, 1, 1)).TotalMilliseconds));
    }
}
