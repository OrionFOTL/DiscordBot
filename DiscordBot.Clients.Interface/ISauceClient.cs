using DiscordBot.Model;

namespace DiscordBot.Services.Interface;

public interface ISauceClient
{
    Task<IEnumerable<SauceData>> GetSauce(string url);
}
