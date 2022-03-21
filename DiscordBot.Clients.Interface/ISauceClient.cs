using DiscordBot.Model;

namespace DiscordBot.Services.Interface;

public interface ISauceClient
{
    Task<IEnumerable<SauceData>> GetSauce(string url);

    Task<IEnumerable<(string Url, IEnumerable<SauceData> Sauces)>> GetSauce(IEnumerable<string> urls);
}
