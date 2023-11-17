using Discord;

namespace DiscordBot.Features.DailyStats;

public record TopAuthor(User Author)
{
    public int MessageCount { get; set; }
}

public record User(ulong Id, string Name)
{
    public User(IMessage message) : this(message.Author.Id, message.Author.Username)
    { }

    public override string ToString() => Name;
};
