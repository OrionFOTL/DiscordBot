using System.Text;

namespace DiscordBot.Features.DailyStats.MessageFormatting;

public interface IMessageFormatter
{
    string FormatTopUsersMessage(IReadOnlyList<User> ranking);
}

internal class MessageFormatter : IMessageFormatter
{
    public string FormatTopUsersMessage(IReadOnlyList<User> ranking)
    {
        if (ranking is null or { Count: 0 })
        {
            return "No active users today.";
        }

        var builder = new StringBuilder();
        builder.AppendLine($"🥇 Congratulations <@{ranking[0].Id}>, you were the most active user today!");

        if (ranking.Count > 1)
        {
            builder.AppendLine($"The second most active user was 🥈<@{ranking[1].Id}>.");
        }

        if (ranking.Count > 2)
        {
            builder.AppendLine($"The third most active user was 🥉<@{ranking[2].Id}>.");
        }

        return builder.ToString();
    }
}