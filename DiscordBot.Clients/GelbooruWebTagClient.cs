using System.Text.Json;
using System.Text.Json.Serialization;
using DiscordBot.Model;
using DiscordBot.Services.Interface;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;

public class GelbooruWebTagClient : ITagClient
{
    private const string _tagApiUrl = @"https://gelbooru.com/index.php?page=autocomplete2&type=tag_query";

    private readonly ILogger<GelbooruWebTagClient> _logger;
    private readonly HttpClient _client;

    public GelbooruWebTagClient(ILogger<GelbooruWebTagClient> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<IEnumerable<Tag>> GetSimilarTags(string tag)
    {
        var tagSearchParameters = new Dictionary<string, string>
        {
            ["limit"] = "5",
            ["term"] = "%" + tag + "%",
        };

        var requestUri = new Uri(QueryHelpers.AddQueryString(_tagApiUrl, tagSearchParameters));

        try
        {
            string apiResponse = await _client.GetStringAsync(requestUri);
            JsonTag[] tags = JsonSerializer.Deserialize<JsonTag[]>(apiResponse);

            return tags?
                .Where(t => t.PostCount > 0)
                .Select(t => new Tag(t.UnderscoredName, t.PostCount, t.Name)) ?? Array.Empty<Tag>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when autocompleting tags for '{tag}'", tag);
            return new[] { new Tag($"Error when autocompleting tags for '{tag}'", 0) };
        }
    }
    
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    private record JsonTag([property: JsonPropertyName("label")] string Name,
                       [property: JsonPropertyName("value")] string UnderscoredName,
                       [property: JsonPropertyName("post_count")] int PostCount);
}