using DiscordBot.Model;

namespace DiscordBot.Services.Interface;

public interface IBooruClient
{
    Task<IEnumerable<Post>> GetImagesAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags);

    Task<Post> GetImageAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags);

    Task<Post> GetRandomImageAsync(bool noVideo = true, bool allowNsfw = false, params string[] contentTags);

    Task<IEnumerable<(string Tag, int Count)>> GetSimilarTags(string tag);
}
