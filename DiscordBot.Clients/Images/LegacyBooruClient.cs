using System.Text;
using BooruDex.Booru;
using BooruDex.Booru.Client;
using BooruDex.Exceptions;
using DiscordBot.Model;
using DiscordBot.Services.Interface;

namespace DiscordBot.Services.Images;

public class LegacyBooruClient : IBooruClient
{
    private readonly Booru _booru;

    public LegacyBooruClient()
    {
        _booru = new Gelbooru();
    }

    public async Task<IEnumerable<Post>> GetImagesAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
    {
        var tags = contentTags.ToList();

        tags.Add(Encoding.UTF8.GetString(Convert.FromBase64String("LWxvbGk=")));

        if (top)
        {
            tags.Add("sort:score");
        }

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
            var posts = await _booru.PostListAsync((byte)amount, tags.ToArray(), (uint)page);

            return posts.Select(p => new Post
            {
                PostUrl = p.PostUrl,
                FileUrl = p.FileUrl,
                PreviewUrl = p.PreviewUrl,
                Tags = p.Tags.Split(','),
                Source = p.Source
            });
        }
        catch (SearchNotFoundException)
        {
            return Array.Empty<Post>();
        }
    }

    public async Task<Post> GetImageAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
    {
        IEnumerable<Post> posts = await GetImagesAsync(amount, page, top, noVideo, allowNsfw, contentTags);

        return posts.FirstOrDefault();
    }

    public async Task<IEnumerable<Tag>> GetSimilarTags(string tag)
    {
        var foundTags = await _booru.TagListAsync(tag.Split('_').First() + "%");

        return foundTags
            .OrderByDescending(t => t.Count)
            .Where(t => t.Name != tag.ToLower())
            .Take(3)
            .Select(t => new Tag(t.Name, t.Count, t.Name));
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
            var post = await _booru.GetRandomPostAsync(contentTags);

            return new Post
            {
                PostUrl = post.PostUrl,
                FileUrl = post.FileUrl,
                PreviewUrl = post.PreviewUrl,
                Tags = post.Tags.Split(','),
                Source = post.Source
            };
        }
        catch (SearchNotFoundException)
        {
            return null;
        }
    }
}
