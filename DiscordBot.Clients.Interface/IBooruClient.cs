using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBot.Model;

namespace DiscordBot.Clients.Interface
{
    public interface IBooruClient
    {
        Task<IEnumerable<Post>> GetImagesAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags);

        Task<Post> GetImageAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags);
    }
}