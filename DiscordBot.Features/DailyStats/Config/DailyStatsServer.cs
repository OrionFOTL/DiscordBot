using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Features.DailyStats.Config;

public record DailyStatsServer
{
    [Required]
    public ulong Id { get; init; }

    [Required]
    public ulong PostingChannelId { get; init; }

    public ulong[] ExcludedChannelIds { get; init; } = [];
}
