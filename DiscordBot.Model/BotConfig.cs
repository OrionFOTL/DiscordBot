namespace DiscordBot.Model;

public class Tokens
{
    public string DiscordBotToken { get; init; }
    public string SaucenaoToken { get; init; }
};

public class GuildConfigs
{
    public List<GuildConfig> Guilds { get; init; }
}

public class GuildConfig
{
    public ulong GuildId { get; init; }
    public ulong? PinChannelId { get; init; }
}
