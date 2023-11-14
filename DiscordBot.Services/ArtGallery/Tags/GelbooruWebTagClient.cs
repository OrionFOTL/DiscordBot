using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.ArtGallery.Tags;

public class GelbooruWebTagClient(ILogger<GelbooruWebTagClient> logger, IHttpClientFactory httpClientFactory) : ITagClient
{
    private readonly Uri _tagApiUri = new(@"https://gelbooru.com/index.php?page=autocomplete2&type=tag_query");

    public async Task<IEnumerable<Tag>> GetSimilarTags(string tag)
    {
        var tagSearchParameters = new Dictionary<string, string>
        {
            ["term"] = "%" + tag + "%",
        };

        var requestUri = _tagApiUri.AddQueryParameters(tagSearchParameters);

        try
        {
            var tags = await httpClientFactory.CreateClient().GetFromJsonAsync<JsonTag[]>(requestUri);

            return tags?
                .Where(t => t.PostCount > 0)
                .Select(t => new Tag(t.UnderscoredName, t.PostCount, t.Name)) ?? Array.Empty<Tag>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when autocompleting tags for '{tag}'", tag);
            return new[] { new Tag($"Error when autocompleting tags for '{tag}'", 0) };
        }
    }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    private record JsonTag([property: JsonPropertyName("label")] string Name,
                           [property: JsonPropertyName("value")] string UnderscoredName,
                           [property: JsonPropertyName("post_count")] int PostCount);
}