namespace DiscordBot.Services.ArtGallery.Images;

public class Art
{
    public required string PostUrl { get; init; }
    public required string FileUrl { get; init; }
    public required string PreviewUrl { get; init; }
    public IEnumerable<string> Tags { get; init; } = [];
    public string? Source { get; set; }
}
