using System;
using System.Linq;

namespace AccountDownloader.Extensions;


// https://stackoverflow.com/questions/372865/path-combine-for-urls
public static class UriExtensions
{
    public static Uri Append(this Uri uri, params string[] paths)
    {
        return new Uri(paths.Aggregate(uri.AbsoluteUri, (current, path) => string.Format("{0}/{1}", current.TrimEnd('/'), path.TrimStart('/'))));
    }
}
