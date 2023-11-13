using System.Globalization;
using Microsoft.Extensions.Options;
using SauceNET;

namespace DiscordBot.Services.ArtGallery.Source;

public class SauceClient : ISauceClient
{
    private const double _minimumSimilarity = 80d;

    private readonly SauceNETClient _sauceClient;

    public SauceClient(IOptions<SaucenaoConfig> options)
    {
        _sauceClient = new SauceNETClient(options.Value.ApiKey);
    }

    public async Task<IEnumerable<SauceData>> GetSauce(string url)
    {
        var response = await _sauceClient.GetSauceAsync(url);

        var sauces = response.Results
            .Where(r => double.Parse(r.Similarity, CultureInfo.InvariantCulture) > _minimumSimilarity)
            .OrderByDescending(r => r.DatabaseName == "Pixiv")
            .ThenByDescending(r => r.DatabaseName == "Twitter")
            .ThenByDescending(r => r.DatabaseName == "E-Hentai")
            .ThenByDescending(r => double.Parse(r.Similarity, CultureInfo.InvariantCulture))
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
