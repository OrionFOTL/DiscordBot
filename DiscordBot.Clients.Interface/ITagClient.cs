namespace DiscordBot.Services.Interface
{
    public interface ITagClient
    {
        Task<IEnumerable<(string Tag, int Count)>> GetSimilarTags(string tag);
    }
}
