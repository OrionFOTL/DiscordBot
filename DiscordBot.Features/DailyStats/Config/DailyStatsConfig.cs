namespace DiscordBot.Features.DailyStats.Config;

public record DailyStatsConfig
{
    public IReadOnlyCollection<DailyStatsServer> Servers { get; set; } = [];
}
