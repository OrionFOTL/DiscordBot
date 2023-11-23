using Discord;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Features.DailyStats.MessageDownloading;

public interface IRecentMessagesFetcher
{
    IAsyncEnumerable<IMessage> GetRecentMessages(IMessageChannel channel, DateTimeOffset oldestDate);
}

internal class RecentMessagesFetcher(ILogger<RecentMessagesFetcher> logger) : IRecentMessagesFetcher
{
    public async IAsyncEnumerable<IMessage> GetRecentMessages(IMessageChannel channel, DateTimeOffset oldestDate)
    {
        logger.LogInformation("Fetching recent messages for channel {channelName} up to {oldestDate}", channel.Name, oldestDate);

        var lastMessage = (await channel.GetMessagesAsync(1).FlattenAsync()).FirstOrDefault();

        if (lastMessage is null || lastMessage.CreatedAt < oldestDate)
        {
            yield break;
        }

        while (lastMessage is not null)
        {
            await foreach (var messageBatch in channel.GetMessagesAsync(lastMessage, Direction.Before, 100))
            {
                if (messageBatch.Count == 0)
                {
                    yield break;
                }

                foreach (var message in messageBatch)
                {
                    if (message.CreatedAt < oldestDate)
                    {
                        yield break;
                    }

                    yield return message;
                    lastMessage = message;
                }
            }
        }
    }
}