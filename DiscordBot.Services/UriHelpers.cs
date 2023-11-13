using System.Web;

namespace DiscordBot.Services;

internal static class UriHelpers
{
    public static Uri AddQueryParameters(this Uri uri, Dictionary<string, string> parameters)
    {
        var uriBuilder = new UriBuilder(uri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var parameter in parameters)
        {
            query[parameter.Key] = parameter.Value;
        }

        uriBuilder.Query = query.ToString();

        return uriBuilder.Uri;
    }
}
