namespace DiscordBot.Model;

public class SauceData
{
    public SauceData()
    {
    }

    public SauceData(string imageUrl, SauceNET.Model.Result result)
    {
        ImageUrl = imageUrl;
        SiteName = result.DatabaseName;
        Site = SiteName switch
        {
            "Pixiv" => Site.Pixiv,
            "Twitter" => Site.Twitter,
            "E-hentai" => Site.Ehentai,
            "Yande.re" => Site.Yandere,
            "Gelbooru" => Site.Gelbooru,
            "Danbooru" => Site.Danbooru,
            _ => Site.Other
        };
        ThumbnailUrl = result.ThumbnailURL;

        switch (Site)
        {
            case Site.Pixiv:
                SourcePostUrl = new Uri(result.SourceURL);
                Title = result.Properties.FirstOrDefault(p => p.Name == "Title")?.Value;
                ArtistName = result.Properties.FirstOrDefault(p => p.Name == "MemberName")?.Value;

                var artistId = result.Properties.FirstOrDefault(p => p.Name == "MemberId")?.Value;
                if (artistId != null)
                {
                    ArtistUrl = new Uri($"https://www.pixiv.net/en/users/{artistId}/artworks");
                }
                break;
            case Site.Twitter:
                SourcePostUrl = new Uri(result.SourceURL);
                Title = "Tweet";
                break;
            case Site.Ehentai:
                SourcePostUrl = new Uri("https://e-hentai.org/?f_search=" + Uri.EscapeDataString(result.InnerSource));
                Title = result.InnerSource;
                break;
            case Site.Yandere:
            case Site.Gelbooru:
            case Site.Danbooru:
            case Site.Other:
            default:
                SourcePostUrl = new Uri(result.SourceURL);
                break;
        }

        LinkedPost = $"[{Title}]({SourcePostUrl.AbsoluteUri})";

        Byline = this switch
        {
            { ArtistName: not null, ArtistUrl: not null } => $"by [{ArtistName}]({ArtistUrl})",
            { ArtistName: not null } => $"by {ArtistName}",
            _ => null
        };
    }

    public string ImageUrl { get; set; }

    public string SiteName { get; set; }

    public Site Site { get; set; }

    public string ThumbnailUrl { get; set; }

    public string Title { get; set; } = "Post";

    public Uri SourcePostUrl { get; set; }

    public string ArtistName { get; set; }

    public Uri ArtistUrl { get; set; }

    public string LinkedPost { get; private set; }

    public string Byline { get; private set; }
}

public enum Site
{
    Pixiv,
    Twitter,
    Ehentai,
    Yandere,
    Gelbooru,
    Danbooru,
    Other
}
