namespace DiscordBot.Services.ArtGallery.Images;

public class Art
{
    public string PostUrl { get; set; }
    public string FileUrl { get; set; }
    public string PreviewUrl { get; set; }
    public IEnumerable<string> Tags { get; set; }
    public string Source { get; set; }
}
