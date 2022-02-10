using BooruSharp.Booru;
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

    public async Task<Post> GetImageAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
    {
        BooruSharp.Search.Post.SearchResult post = await _gelbooru.GetRandomPostAsync(contentTags);

        return new Post
        {
            FileUrl = post.FileUrl.ToString(),
            PostUrl = post.PostUrl.ToString(),
            PreviewUrl = post.PreviewUrl.ToString(),
            Source = post.Source,
            Tags = post.Tags.Aggregate((s1, s2) => string.Join(", ", s1, s2)),
        };
    }

    public Task<IEnumerable<Post>> GetImagesAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> GetTopTags(string tag)
    {
        BooruSharp.Search.Tag.SearchResult[] similarTags = await _gelbooru.GetTagsAsync(tag);

        return similarTags.Select(t => t.Name);
    }
}
