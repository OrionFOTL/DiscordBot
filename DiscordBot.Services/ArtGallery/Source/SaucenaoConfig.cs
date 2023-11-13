using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Services.ArtGallery.Source;

public class SaucenaoConfig
{
    [Required]
    public required string ApiKey { get; init; }
}
