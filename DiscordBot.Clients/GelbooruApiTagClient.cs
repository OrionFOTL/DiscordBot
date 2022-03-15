using System.Text.Json;
using System.Text.Json.Serialization;
using DiscordBot.Model;
using DiscordBot.Services.Interface;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;

public class GelbooruApiTagClient : ITagClient
{
    private const string _tagApiUrl = @"https://gelbooru.com/index.php?page=dapi&s=tag&q=index&json=1";

    private readonly ILogger<GelbooruApiTagClient> _logger;
    private readonly HttpClient _client;

    public GelbooruApiTagClient(ILogger<GelbooruApiTagClient> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<IEnumerable<Tag>> GetSimilarTags(string tag)
    {
        var tagSearchParameters = new Dictionary<string, string>
        {
            ["limit"] = "5",
            ["orderby"] = "count",
            ["name_pattern"] = "%" + tag + "%",
        };

        var requestUri = new Uri(QueryHelpers.AddQueryString(_tagApiUrl, tagSearchParameters));

        try
        {
            string response = await _client.GetStringAsync(requestUri);
            TagResponse tagResponse = JsonSerializer.Deserialize<TagResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return tagResponse.Tags?
                .Where(t => t.Count > 0)
                .Select(t => new Tag(t.Name, t.Count)) ?? Array.Empty<Tag>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when autocompleting tags for '{tag}'", tag);
            return new[] { new Tag($"Error when autocompleting tags for '{tag}'", 0) };
        }
    }

    private record TagResponse([property: JsonPropertyName("@attributes")] Attributes Attributes,
                               [property: JsonPropertyName("tag")] List<ApiTag> Tags);

    private record Attributes(int Limit, int Offset, int Count);

    private record ApiTag(int Id, string Name, int Count, int Type, int Ambiguous);
}
