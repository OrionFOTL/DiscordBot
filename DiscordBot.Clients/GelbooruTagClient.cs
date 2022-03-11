using System.Text.Json;
using System.Text.Json.Serialization;
using DiscordBot.Services.Interface;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;

public class GelbooruTagClient : ITagClient
{
    private readonly ILogger<GelbooruTagClient> _logger;
    private readonly HttpClient _client;

    public GelbooruTagClient(ILogger<GelbooruTagClient> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<IEnumerable<(string Tag, int Count)>> GetSimilarTags(string tag)
    {
        var uri = new Uri($"https://gelbooru.com/index.php?page=dapi&s=tag&q=index&json=1&limit=5&orderby=count&name_pattern=%{tag}%");
        string response;

        try
        {
            response = await _client.GetStringAsync(uri);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when autocompleting tags for '{tag}'", tag);
            return new[] { (Tag: $"Error when autocompleting tags for '{tag}'", Count: 0) };
        }

        TagResponse tagResponse = JsonSerializer.Deserialize<TagResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return tagResponse.Tags.Where(t => t.Count > 0).Select(t => (Tag: t.Name, t.Count));
    }
    
    class TagResponse
    {
        [JsonPropertyName("@attributes")]
        public Attributes Attributes { get; set; }
    
        [JsonPropertyName("tag")]
        public List<Tag> Tags { get; set; }
    }
    class Attributes
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public int Count { get; set; }
    }

    class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int Type { get; set; }
        public int Ambiguous { get; set; }
    }
}