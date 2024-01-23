using Discord;
using DiscordBot.Features.DailyStats.MessageDownloading;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Features.DailyStats.UserActivity;

public interface IUserActivityAnalyser
{
    string Title { get; }

    Task<IReadOnlyList<TopAuthor>> GetMostActiveUsers(IEnumerable<ITextChannel> channels, DateTimeOffset oldestDate);
}

internal abstract class UserActivityAnalyser(
    ILogger<UserActivityAnalyser> logger,
    IRecentMessagesFetcher recentMessagesFetcher)
{
    public async Task<IReadOnlyList<TopAuthor>> GetMostActiveUsers(IEnumerable<ITextChannel> channels, DateTimeOffset oldestDate)
    {
        var topAuthors = new Dictionary<ulong, TopAuthor>();

        foreach (var channel in channels)
        {
            try
            {
                await foreach (var message in recentMessagesFetcher.GetRecentMessages(channel, oldestDate))
                {
                    int score = CalculateScore(message);

                    if (topAuthors.TryGetValue(message.Author.Id, out var existingTopAuthor))
                    {
                        existingTopAuthor.Score += score;
                    }
                    else
                    {
                        topAuthors[message.Author.Id] = new TopAuthor(new User(message)) { Score = score };
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Couldn't get recent messages from channel {channelId}:{channelName}; skipping channel", channel.Id, channel.Name);
            }
        }

        logger.LogInformation("Calculated {topAuthorsCount} top authors", topAuthors.Count);

        return topAuthors.Values.OrderByDescending(a => a.Score).ToList();
    }

    protected abstract int CalculateScore(IMessage message);
}

internal class PostsCountUserActivityAnalyser(
    ILogger<UserActivityAnalyser> logger,
    IRecentMessagesFetcher recentMessagesFetcher) : UserActivityAnalyser(logger, recentMessagesFetcher), IUserActivityAnalyser
{
    public string Title => "Users with most messages in the last 24 hours";

    protected override int CalculateScore(IMessage message) => 1;
}

internal class WordCountUserActivityAnalyser(
    ILogger<UserActivityAnalyser> logger,
    IRecentMessagesFetcher recentMessagesFetcher) : UserActivityAnalyser(logger, recentMessagesFetcher), IUserActivityAnalyser
{
    public string Title => "Users with most words in the last 24 hours";

    protected override int CalculateScore(IMessage message) => message.Content.Split(' ').Length;
}
