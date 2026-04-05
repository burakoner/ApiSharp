namespace ApiSharp.Extensions;

/// <summary>
/// Dictionary extensions
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Add a parameter
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void AddParameter(this Dictionary<string, object> parameters, string key, string value)
    {
        parameters.Add(key, value);
    }

    /// <summary>
    /// Add a parameter
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="converter"></param>
    public static void AddParameter(this Dictionary<string, object> parameters, string key, string value, JsonConverter converter)
    {
        parameters.Add(key, JsonConvert.SerializeObject(value, converter));
    }

    /// <summary>
    /// Add a parameter
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void AddParameter(this Dictionary<string, object> parameters, string key, object value)
    {
        parameters.Add(key, value);
    }

    /// <summary>
    /// Add a parameter
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="converter"></param>
    public static void AddParameter(this Dictionary<string, object> parameters, string key, object value, JsonConverter converter)
    {
        parameters.Add(key, JsonConvert.SerializeObject(value, converter));
    }

    /// <summary>
    /// Add an optional parameter. Not added if value is null
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object? value)
    {
        if (value != null)
        {
            if (value is string str)
            {
                if (!string.IsNullOrWhiteSpace(str) && str != "null")
                {
                    parameters.Add(key, value);
                }
            }
            else
            {
                parameters.Add(key, value);
            }
        }
    }

    /// <summary>
    /// Add an optional parameter. Not added if value is null
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="converter"></param>
    public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object? value, JsonConverter converter)
    {
        if (value != null)
            parameters.Add(key, JsonConvert.SerializeObject(value, converter));
    }

    /// <summary>
    /// Add an optional parameter. Not added if value is null
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public static void AddOptionalParameter(this Dictionary<string, string> parameters, string key, string value)
    {
        if (value != null)
            parameters.Add(key, value);
    }

    /// <summary>
    /// Add an optional parameter. Not added if value is null
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="converter"></param>
    public static void AddOptionalParameter(this Dictionary<string, string> parameters, string key, string value, JsonConverter converter)
    {
        if (value != null)
            parameters.Add(key, JsonConvert.SerializeObject(value, converter));
    }

    /// <summary>
    /// Create a query string of the specified parameters
    /// </summary>
    /// <param name="parameters">The parameters to use</param>
    /// <param name="urlEncodeValues">Whether or not the values should be url encoded</param>
    /// <param name="serializationType">How to serialize array parameters</param>
    /// <returns></returns>
    public static string CreateParamString(this Dictionary<string, object> parameters, bool urlEncodeValues, ArraySerialization serializationType)
    {
        var parts = new List<string>();
        foreach (var parameter in parameters)
        {
            if (parameter.Value is Array array)
            {
                foreach (var item in array)
                {
                    var key = serializationType == ArraySerialization.Array ? parameter.Key + "[]" : parameter.Key;
                    var value = item?.ToString() ?? string.Empty;
                    if (urlEncodeValues)
                        value = Uri.EscapeDataString(value);

                    parts.Add($"{key}={value}");
                }
            }
            else
            {
                var value = parameter.Value?.ToString() ?? string.Empty;
                if (urlEncodeValues)
                    value = Uri.EscapeDataString(value);

                parts.Add($"{parameter.Key}={value}");
            }
        }

        return string.Join("&", parts);
    }

    /// <summary>
    /// Convert a dictionary to formdata string
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static string ToFormData(this SortedDictionary<string, object> parameters)
    {
        var formData = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kvp in parameters)
        {
            if (kvp.Value.GetType().IsArray)
            {
                var array = (Array)kvp.Value;
                foreach (var value in array)
                    formData.Add(kvp.Key, value.ToString());
            }
            else
                formData.Add(kvp.Key, kvp.Value.ToString());
        }
        return formData.ToString();
    }

    /// <summary>
    /// Convert a dictionary to formdata string
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static string ToFormData(this Dictionary<string, object> parameters)
    {
        var formData = HttpUtility.ParseQueryString(string.Empty);
        foreach (var kvp in parameters)
        {
            if (kvp.Value.GetType().IsArray)
            {
                var array = (Array)kvp.Value;
                foreach (var value in array)
                    formData.Add(kvp.Key, value.ToString());
            }
            else
                formData.Add(kvp.Key, kvp.Value.ToString());
        }
        return formData.ToString();
    }
}
