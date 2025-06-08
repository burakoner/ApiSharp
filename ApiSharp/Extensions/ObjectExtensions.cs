namespace ApiSharp.Extensions;

/// <summary>
/// Object extensions for working with collections
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Gets the index of a sequence within another sequence.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    public static int IndexOf<T>(this IEnumerable<T> source, IEnumerable<T> search)
    {
        var index = -1;
        for (var i = 0; i <= source.Count() - search.Count(); i++)
        {
            var matched = true;
            for (var j = 0; j < search.Count(); j++)
            {
                matched = matched && source.ElementAt(i + j)!.Equals(search.ElementAt(j));
            }
            if (matched)
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
