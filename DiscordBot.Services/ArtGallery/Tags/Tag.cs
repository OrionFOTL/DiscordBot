namespace DiscordBot.Services.ArtGallery.Tags;

public readonly record struct Tag(string CodedName, int Count, string? PrettyName = null)
{
    public string PrettyName { get; init; } = PrettyName ?? CodedName;
}