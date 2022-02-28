﻿using BooruSharp.Booru;
using DiscordBot.Model;
using DiscordBot.Services.Interface;

namespace DiscordBot.Services;

public class NewBooruClient : IBooruClient
{
    private readonly Gelbooru _gelbooru;

    public NewBooruClient()
    {
        _gelbooru = new Gelbooru();
    }

    public Task<Post> GetImageAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Post>> GetImagesAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
    {
        throw new NotImplementedException();
    }

    public async Task<Post> GetRandomImageAsync(bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
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

            return new Post
            {
                FileUrl = post.FileUrl.ToString(),
                PostUrl = post.PostUrl.ToString(),
                PreviewUrl = post.PreviewUrl.ToString(),
                Source = post.Source,
                Tags = post.Tags,
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<IEnumerable<(string Tag, int Count)>> GetSimilarTags(string tag)
    {
        BooruSharp.Search.Tag.SearchResult[] similarTags = await _gelbooru.GetTagsAsync(tag);

        return similarTags.Select(t => (Tag: t.Name, t.Count));
    }
}
