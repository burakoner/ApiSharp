namespace ApiSharp.Extensions;

public static class UrlExtensions
{
    /// <summary>
    /// Append a base url with provided path
    /// </summary>
    /// <param name="url"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string AppendPath(this string url, params string[] path)
    {
        if (!url.EndsWith("/"))
            url += "/";

        foreach (var item in path)
            url += item.Trim('/') + "/";

        return url.TrimEnd('/');
    }

    /// <summary>
    /// Fill parameters in a path. Parameters are specified by '{}' and should be specified in occuring sequence
    /// </summary>
    /// <param name="path">The total path string</param>
    /// <param name="values">The values to fill</param>
    /// <returns></returns>
    public static string FillPathParameters(this string path, params string[] values)
    {
        foreach (var value in values)
        {
            var index = path.IndexOf("{}", StringComparison.Ordinal);
            if (index >= 0)
            {
                path = path.Remove(index, 2);
                path = path.Insert(index, value);
            }
        }
        return path;
    }

    /// <summary>
    /// Create a new uri with the provided parameters as query
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="baseUri"></param>
    /// <param name="arraySerialization"></param>
    /// <returns></returns>
    public static Uri SetParameters(this Uri baseUri, SortedDictionary<string, object> parameters, ArraySerialization arraySerialization)
    {
        var uriBuilder = new UriBuilder();
        uriBuilder.Scheme = baseUri.Scheme;
        uriBuilder.Host = baseUri.Host;
        uriBuilder.Port = baseUri.Port;
        uriBuilder.Path = baseUri.AbsolutePath;
        var httpValueCollection = HttpUtility.ParseQueryString(string.Empty);
        foreach (var parameter in parameters)
        {
            if (parameter.Value.GetType().IsArray)
            {
                foreach (var item in (object[])parameter.Value)
                    httpValueCollection.Add(arraySerialization == ArraySerialization.Array ? parameter.Key + "[]" : parameter.Key, item.ToString());
            }
            else
                httpValueCollection.Add(parameter.Key, parameter.Value.ToString());
        }
        uriBuilder.Query = httpValueCollection.ToString();
        return uriBuilder.Uri;
    }

    /// <summary>
    /// Create a new uri with the provided parameters as query
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="baseUri"></param>
    /// <param name="arraySerialization"></param>
    /// <returns></returns>
    public static Uri SetParameters(this Uri baseUri, IOrderedEnumerable<KeyValuePair<string, object>> parameters, ArraySerialization arraySerialization)
    {
        var uriBuilder = new UriBuilder();
        uriBuilder.Scheme = baseUri.Scheme;
        uriBuilder.Host = baseUri.Host;
        uriBuilder.Port = baseUri.Port;
        uriBuilder.Path = baseUri.AbsolutePath;
        var httpValueCollection = HttpUtility.ParseQueryString(string.Empty);
        foreach (var parameter in parameters)
        {
            if (parameter.Value.GetType().IsArray)
            {
                foreach (var item in (object[])parameter.Value)
                    httpValueCollection.Add(arraySerialization == ArraySerialization.Array ? parameter.Key + "[]" : parameter.Key, item.ToString());
            }
            else
                httpValueCollection.Add(parameter.Key, parameter.Value.ToString());
        }
        uriBuilder.Query = httpValueCollection.ToString();
        return uriBuilder.Uri;
    }

    /// <summary>
    /// Add parameter to URI
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Uri AddQueryParmeter(this Uri uri, string name, string value)
    {
        var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

        httpValueCollection.Remove(name);
        httpValueCollection.Add(name, value);

        var ub = new UriBuilder(uri);
        ub.Query = httpValueCollection.ToString();

        return ub.Uri;
    }
}
