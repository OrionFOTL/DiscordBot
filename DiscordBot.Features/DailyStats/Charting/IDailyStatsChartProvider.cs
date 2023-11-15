namespace DiscordBot.Features.DailyStats.Charting;

public interface IDailyStatsChartProvider
{
    Stream GetDailyActivityChart(IReadOnlyList<KeyValuePair<string, int>> ranking);
}
