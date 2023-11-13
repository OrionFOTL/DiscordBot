namespace DiscordBot.Services.ArtGallery.Tags;

public interface ITagClient
{
    Task<IEnumerable<Tag>> GetSimilarTags(string tag);
}
