using DiscordBot.Model;

namespace DiscordBot.Services.Interface
{
    public interface ITagClient
    {
        Task<IEnumerable<Tag>> GetSimilarTags(string tag);
    }
}
