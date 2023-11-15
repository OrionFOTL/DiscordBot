using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Features.DailyStats;

public class DailyStatsConfig
{
    [Required]
    public required string Cron { get; init; }

    [Required]
    public required Server[] Servers { get; init; }
}

public class Server
{
    [Required]
    public ulong Id { get; init; }

    [Required]
    public ulong PostingChannelId { get; init; }

    public ulong[] ExcludedChannelIds { get; init; } = [];
}
