using System.Globalization;
using DiscordBot.Model;
using DiscordBot.Services.Interface;
using Microsoft.Extensions.Configuration;
using SauceNET;

namespace DiscordBot.Services.Source;

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
            .DistinctBy(r => r.DatabaseName);

        //sauces = sauces.Where(s => s.DatabaseName is "Pixiv" or "Twitter" or "E-hentai" or "Yande.re");

        return sauces.Select(s => new SauceData(url, s));
    }

    public async Task<IEnumerable<(string Url, IEnumerable<SauceData> Sauces)>> GetSauce(IEnumerable<string> urls)
    {
        var urlsWithSauceTasks = urls.Select(url => (Url: url, SaucesTask: GetSauce(url))).ToList();

        await Task.WhenAll(urlsWithSauceTasks.Select(url => url.SaucesTask));

        return await Task.WhenAll(urlsWithSauceTasks.Select(async ust => (ust.Url, Sauces: await ust.SaucesTask)));
    }
}
