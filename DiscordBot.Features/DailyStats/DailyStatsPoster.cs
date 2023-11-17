using Discord;
using DiscordBot.Features.DailyStats.Charting;
using DiscordBot.Features.DailyStats.Config;
using DiscordBot.Features.DailyStats.MessageFormatting;
using DiscordBot.Features.DailyStats.UserActivity;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Features.DailyStats;

public interface IDailyStatsPoster
{
    Task PostMostActiveUsersToday(DailyStatsServer server);
}

internal class DailyStatsPoster(
    ILogger<DailyStatsPoster> logger,
    IDiscordClient discordClient,
    IUserActivityAnalyser userActivityAnalyser,
    IDailyStatsChartProvider dailyStatsChartProvider,
    IMessageFormatter messageFormatter) : IDailyStatsPoster
{
    public async Task PostMostActiveUsersToday(DailyStatsServer server)
    {
        var guild = await discordClient.GetGuildAsync(server.Id);
        logger.LogInformation("About to post most active users for server {serverId}:{serverName}", guild.Id, guild.Name);
        var botUser = await guild.GetCurrentUserAsync();
        var textChannels = await guild.GetTextChannelsAsync();

        var filteredTextChannels = from channel in textChannels
                                   let permissions = botUser.GetPermissions(channel)
                                   where !server.ExcludedChannelIds.Contains(channel.Id)
                                         && permissions.ViewChannel
                                         && permissions.ReadMessageHistory
                                         && ((channel is not IVoiceChannel) || (channel is IVoiceChannel && permissions.Connect))
                                   select channel;

        var startDate = DateTimeOffset.Now.AddDays(-1);

        var topAuthors = await userActivityAnalyser.GetMostActiveUsersByRecentPostsCount(filteredTextChannels, startDate);

        using Stream chartStream = dailyStatsChartProvider.GetDailyActivityChart(topAuthors);

        var postingChannel = await guild.GetTextChannelAsync(server.PostingChannelId);

        await postingChannel.SendFileAsync(chartStream, "chart.png", messageFormatter.FormatTopUsersMessage(topAuthors.Select(a => a.Author).ToList()));

        logger.LogInformation("Posted most active users to channel {channelId}:{channelName}", postingChannel.Id, postingChannel.Name);
    }
}