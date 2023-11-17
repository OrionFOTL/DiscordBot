using Discord;
using DiscordBot.Features.DailyStats.MessageDownloading;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Features.DailyStats.UserActivity;

public interface IUserActivityAnalyser
{
    Task<IReadOnlyList<TopAuthor>> GetMostActiveUsersByRecentPostsCount(IEnumerable<ITextChannel> channels, DateTimeOffset oldestDate);
}

internal class UserActivityAnalyser(
    ILogger<UserActivityAnalyser> logger,
    IRecentMessagesFetcher recentMessagesFetcher) : IUserActivityAnalyser
{
    public async Task<IReadOnlyList<TopAuthor>> GetMostActiveUsersByRecentPostsCount(IEnumerable<ITextChannel> channels, DateTimeOffset oldestDate)
    {
        var topAuthors = new Dictionary<ulong, TopAuthor>();

        foreach (var channel in channels)
        {
            try
            {
                await foreach (var message in recentMessagesFetcher.GetRecentMessages(channel, oldestDate))
                {
                    if (topAuthors.TryGetValue(message.Author.Id, out var existingTopAuthor))
                    {
                        existingTopAuthor.MessageCount++;
                    }
                    else
                    {
                        topAuthors[message.Author.Id] = new TopAuthor(new User(message)) { MessageCount = 1 };
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Couldn't get recent messages from channel {channelId}:{channelName}; skipping channel", channel.Id, channel.Name);
            }
        }

        logger.LogInformation("Calculated {topAuthorsCount} top authors", topAuthors.Count);

        return topAuthors.Values.OrderByDescending(a => a.MessageCount).ToList();
    }
}