namespace ApiSharp.Extensions;

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
    public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object value)
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
    public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object value, JsonConverter converter)
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
        var uriString = string.Empty;
        var arraysParameters = parameters.Where(p => p.Value.GetType().IsArray).ToList();
        foreach (var arrayEntry in arraysParameters)
        {
            if (serializationType == ArraySerialization.Array)
                uriString += $"{string.Join("&", ((object[])(urlEncodeValues ? Uri.EscapeDataString(arrayEntry.Value.ToString()) : arrayEntry.Value)).Select(v => $"{arrayEntry.Key}[]={v}"))}&";
            else
            {
                var array = (Array)arrayEntry.Value;
                uriString += string.Join("&", array.OfType<object>().Select(a => $"{arrayEntry.Key}={Uri.EscapeDataString(a.ToString())}"));
                uriString += "&";
            }
        }

        uriString += $"{string.Join("&", parameters.Where(p => !p.Value.GetType().IsArray).Select(s => $"{s.Key}={(urlEncodeValues ? Uri.EscapeDataString(s.Value.ToString()) : s.Value)}"))}";
        uriString = uriString.TrimEnd('&');
        return uriString;
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
}
