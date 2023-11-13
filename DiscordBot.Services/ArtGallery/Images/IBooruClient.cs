namespace DiscordBot.Services.ArtGallery.Images;

public interface IBooruClient
{
    Task<IEnumerable<Art>> GetImagesAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags);

    Task<Art> GetImageAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags);

    Task<Art> GetRandomImageAsync(bool noVideo = true, bool allowNsfw = false, params string[] contentTags);
}
