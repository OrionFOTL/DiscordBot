using System.Collections.Immutable;
using Discord;
using DiscordBot.Features.DailyStats.Charting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace DiscordBot.Features.DailyStats;

public class DailyStatsJob(
    ILogger<DailyStatsJob> logger,
    IOptions<DailyStatsConfig> options,
    IDiscordClient discordClient,
    IDailyStatsChartProvider dailyStatsChartProvider) : IJob
{
    private readonly DailyStatsConfig _config = options.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Running {jobName}", nameof(DailyStatsJob));

        if (discordClient.ConnectionState is not ConnectionState.Connected)
        {
            logger.LogError("DiscordClient ConnectionState is {connectionState}", discordClient.ConnectionState);
            return;
        }

        foreach (var serverConfig in _config.Servers)
        {
            var guild = await discordClient.GetGuildAsync(serverConfig.Id);
            var textChannels = await guild.GetTextChannelsAsync();
            var filteredTextChannels = textChannels.Where(tc => !serverConfig.ExcludedChannelIds.Contains(tc.Id)).ToList();

            var startDate = DateTimeOffset.Now.AddDays(-1);
            logger.LogInformation("Fetching recent messages for guild {guildName}", guild.Name);

            var lastMessageAuthors = await filteredTextChannels
                .ToAsyncEnumerable()
                .SelectMany(ch => GetLast24hMessages(ch, startDate).Select(m => new Author(m.Author.Id, m.Author.Username)))
                .ToListAsync();

            if (lastMessageAuthors.Count is 0)
            {
                continue;
            }

            var topAuthors = lastMessageAuthors
                .GroupBy(a => a.Id)
                .Select(g => new { Author = g.First(), Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToList();

            using Stream chartStream = dailyStatsChartProvider.GetDailyActivityChart(
                topAuthors.Select(x => new KeyValuePair<string, int>(x.Author.Name, x.Count))
                          .ToList());

            var postingChannel = await guild.GetTextChannelAsync(serverConfig.PostingChannelId);

            await postingChannel.SendFileAsync(
                chartStream,
                "chart.png",
                $"""
                🥇 Congratulations <@{topAuthors.First().Author.Name}>, you were the most active user today!
                The second and third most active users were 🥈<@{topAuthors[1].Author.Id}> and 🥉<@{topAuthors[2].Author.Id}>.
                """);
        }
    }

    private async IAsyncEnumerable<IMessage> GetLast24hMessages(ITextChannel channel, DateTimeOffset startDate)
    {
        logger.LogInformation("Fetching recent messages for channel {channelName}", channel.Name);

        var lastMessage = (await channel.GetMessagesAsync(1).FlattenAsync()).FirstOrDefault();

        if (lastMessage is null || lastMessage.CreatedAt < startDate)
        {
            yield break;
        }

        while (lastMessage is not null)
        {
            await foreach (var messageBatch in channel.GetMessagesAsync(lastMessage, Direction.Before, 100))
            {
                foreach (var message in messageBatch)
                {
                    if (message.CreatedAt < startDate)
                    {
                        yield break;
                    }

                    yield return message;
                    lastMessage = message;
                }
            }
        }
    }

    private record Author(ulong Id, string Name);
}
