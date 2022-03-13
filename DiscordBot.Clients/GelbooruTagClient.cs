using System.Text.Json;
using System.Text.Json.Serialization;
using DiscordBot.Services.Interface;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;

public class GelbooruTagClient : ITagClient
{
    private const string _tagApiUrl = @"https://gelbooru.com/index.php?page=dapi&s=tag&q=index&json=1";

    private readonly ILogger<GelbooruTagClient> _logger;
    private readonly HttpClient _client;

    public GelbooruTagClient(ILogger<GelbooruTagClient> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<IEnumerable<(string Tag, int Count)>> GetSimilarTags(string tag)
    {
        var tagSearchParameters = new Dictionary<string, string>
        {
            ["limit"] = "5",
            ["orderby"] = "count",
            ["name_pattern"] = "%" + tag + "%",
        };

        var requestUri = new Uri(QueryHelpers.AddQueryString(_tagApiUrl, tagSearchParameters));

        string response;

        try
        {
            response = await _client.GetStringAsync(requestUri);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when autocompleting tags for '{tag}'", tag);
            return new[] { (Tag: $"Error when autocompleting tags for '{tag}'", Count: 0) };
        }

        TagResponse tagResponse = JsonSerializer.Deserialize<TagResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return tagResponse.Tags.Where(t => t.Count > 0).Select(t => (Tag: t.Name, t.Count));
    }

    private record TagResponse([property:JsonPropertyName("@attributes")] Attributes Attributes, [property: JsonPropertyName("tag")] List<Tag> Tags);

    private record Attributes(int Limit, int Offset, int Count);

    private record Tag(int Id, string Name, int Count, int Type, int Ambiguous);
}