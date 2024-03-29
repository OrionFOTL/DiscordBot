﻿using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.ArtGallery.Tags;

public class GelbooruApiTagClient(
    ILogger<GelbooruApiTagClient> logger,
    IHttpClientFactory httpClientFactory) : ITagClient
{
    private readonly Uri _tagApiUri = new(@"https://gelbooru.com/index.php?page=dapi&s=tag&q=index&json=1");

    public async Task<IEnumerable<Tag>> GetSimilarTags(string tag)
    {
        var tagSearchParameters = new Dictionary<string, string>
        {
            ["limit"] = "5",
            ["orderby"] = "count",
            ["name_pattern"] = "%" + tag + "%",
        };

        var requestUri = _tagApiUri.AddQueryParameters(tagSearchParameters);

        try
        {
            var tagResponse = await httpClientFactory
                .CreateClient()
                .GetFromJsonAsync<TagResponse>(requestUri, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return tagResponse?
                .Tags?
                .Where(t => t.Count > 0)
                .Select(t => new Tag(t.Name, t.Count)) ?? Enumerable.Empty<Tag>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when autocompleting tags for '{tag}'", tag);
            return new[] { new Tag($"Error when autocompleting tags for '{tag}'", 0) };
        }
    }

    private record TagResponse([property: JsonPropertyName("@attributes")] Attributes Attributes,
                               [property: JsonPropertyName("tag")] List<ApiTag> Tags);

    private record Attributes(int Limit, int Offset, int Count);

    private record ApiTag(int Id, string Name, int Count, int Type, int Ambiguous);
}
