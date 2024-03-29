﻿using BooruSharp.Booru;
using DiscordBot.Services.ArtGallery.Tags;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.ArtGallery.Images;

public class NewGelbooruClient(ILogger<NewGelbooruClient> logger) : IBooruClient
{
    private readonly Gelbooru _gelbooru = new();
    private readonly ILogger<NewGelbooruClient> _logger = logger;

    public Task<Art> GetImageAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Art>> GetImagesAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
    {
        throw new NotImplementedException();
    }

    public async Task<Art?> GetRandomImageAsync(bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
    {
        var tags = contentTags.ToList();

        if (noVideo)
        {
            tags.Add("-video -mp4 -webm");
        }

        if (!allowNsfw)
        {
            tags.Add("rating:safe");
        }

        try
        {
            BooruSharp.Search.Post.SearchResult post = await _gelbooru.GetRandomPostAsync(contentTags);

            return new Art
            {
                FileUrl = post.FileUrl.ToString(),
                PostUrl = post.PostUrl.ToString(),
                PreviewUrl = post.PreviewUrl.ToString(),
                Source = post.Source,
                Tags = post.Tags,
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception occured during getting image for tags: {tags}", contentTags);
            return null;
        }
    }

    public async Task<IEnumerable<Tag>> GetSimilarTags(string tag)
    {
        BooruSharp.Search.Tag.SearchResult[] similarTags = await _gelbooru.GetTagsAsync(tag);

        return similarTags.Select(t => new Tag(t.Name, t.Count));
    }
}
