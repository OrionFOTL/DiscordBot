using Discord;

namespace DiscordBot.Features.DailyStats;

public record TopAuthor(User Author)
{
    public int Score { get; set; }
}

public record User(ulong Id, string Name)
{
    public User(IMessage message) : this(message.Author.Id, message.Author.Username)
    { }

    public override string ToString() => Name;
};
