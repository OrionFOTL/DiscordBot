using System.ComponentModel.DataAnnotations;

namespace DiscordBot;

public class BotConfig
{
    [Required]
    public required string BotToken { get; init; }
};
