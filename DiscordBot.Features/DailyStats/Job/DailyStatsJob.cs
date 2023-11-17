using Discord;
using DiscordBot.Features.DailyStats.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace DiscordBot.Features.DailyStats.Job;

[DisallowConcurrentExecution]
public class DailyStatsJob(
    ILogger<DailyStatsJob> logger,
    IOptions<DailyStatsConfig> serverConfig,
    IDiscordClient discordClient,
    IDailyStatsPoster dailyStatsPoster) : IJob
{
    private readonly DailyStatsConfig _serverConfig = serverConfig.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Running {jobName}", nameof(DailyStatsJob));

        if (discordClient.ConnectionState is not ConnectionState.Connected)
        {
            logger.LogWarning("DiscordClient ConnectionState is {connectionState}", discordClient.ConnectionState);
            return;
        }

        foreach (var serverConfig in _serverConfig.Servers)
        {
            try
            {
                await dailyStatsPoster.PostMostActiveUsersToday(serverConfig);
            }
            catch (Exception e)
            {
                var guild = await discordClient.GetGuildAsync(serverConfig.Id);
                logger.LogError(e, "Couldn't post most active users for server {serverId}:{serverName}", serverConfig.Id, guild.Name);
            }
        }
    }
}
