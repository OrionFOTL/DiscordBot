using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBot.Model;

namespace DiscordBot.Clients.Interface
{
    public interface ISauceClient
    {
        Task<IEnumerable<SauceData>> GetSauce(string url);
    }
}