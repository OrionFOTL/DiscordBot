using System.Globalization;
using DiscordBot.Clients.Interface;
using DiscordBot.Model;
using Microsoft.Extensions.Configuration;
using SauceNET;

namespace DiscordBot.Clients;

public class SauceClient : ISauceClient
{
    private const double _minimumSimilarity = 80d;

    private readonly SauceNETClient _sauceClient;

    public SauceClient(IConfiguration configuration)
    {
        _sauceClient = new SauceNETClient(configuration["SaucenaoToken"]);
    }

    public async Task<IEnumerable<SauceData>> GetSauce(string url)
    {
        var response = await _sauceClient.GetSauceAsync(url);

        var sauces = response.Results
            .Where(r => Convert.ToDouble(r.Similarity, CultureInfo.InvariantCulture) > _minimumSimilarity)
            .OrderByDescending(r => r.DatabaseName == "Pixiv")
            .ThenByDescending(r => r.DatabaseName == "Twitter")
            .ThenByDescending(r => r.DatabaseName == "E-Hentai")
            .ThenByDescending(r => Convert.ToDouble(r.Similarity, CultureInfo.InvariantCulture))
            .GroupBy(r => r.DatabaseName)
            .Select(group => group.First());

        sauces = sauces.Where(s => new[] { "Pixiv", "Twitter", "E-hentai", "Yande.re" }.Contains(s.DatabaseName));

        return sauces.Select(s => new SauceData
        {
            Title = s.Properties.FirstOrDefault(p => p.Name == "Title" || (s.DatabaseName == "E-hentai" && p.Name == "Source"))?.Value,
            ArtistName = s.Properties.FirstOrDefault(p => p.Name is "MemberName" or "Creator")?.Value,
            ArtistId = s.Properties.FirstOrDefault(p => p.Name == "MemberId")?.Value,
            SourcePostUrl = s.DatabaseName == "E-hentai" ? Uri.EscapeUriString("https://e-hentai.org/?f_search=" + s.InnerSource) : s.SourceURL,
            ThumbnailUrl = s.ThumbnailURL,
            SiteName = s.DatabaseName,
        });
    }
}
