using Discord.Interactions;
using DiscordBot.Features.DailyStats.Config;
using Microsoft.Extensions.Options;

namespace DiscordBot.Features.DailyStats.Command;

public class DailyStatsCommand(
    IOptions<DailyStatsConfig> options,
    IDailyStatsPoster dailyStatsPoster) : InteractionModuleBase
{
    private readonly DailyStatsConfig _dailyStatsConfig = options.Value;

    [RequireOwner]
    [SlashCommand("daily_stats", "Post this server's Daily Stats")]
    public async Task PostDailyStats()
    {
        await RespondAsync("Generating", ephemeral: true);

        var knownGuildConfig = _dailyStatsConfig.Servers.FirstOrDefault(s => s.Id == Context.Guild.Id);

        var guildConfig = new DailyStatsServer()
        {
            Id = knownGuildConfig?.Id ?? Context.Guild.Id,
            PostingChannelId = knownGuildConfig?.PostingChannelId ?? Context.Channel.Id,
            ExcludedChannelIds = knownGuildConfig?.ExcludedChannelIds ?? [],
        };

        await dailyStatsPoster.PostMostActiveUsersToday(guildConfig);
    }
}
