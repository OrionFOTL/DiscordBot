using DiscordBot.Common;
using DiscordBot.Features.DailyStats.Charting;
using DiscordBot.Features.DailyStats.Config;
using DiscordBot.Features.DailyStats.Job;
using DiscordBot.Features.DailyStats.MessageDownloading;
using DiscordBot.Features.DailyStats.MessageFormatting;
using DiscordBot.Features.DailyStats.UserActivity;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Features.DailyStats.ServiceCollectionExtension;

public static class DailyStatsServiceCollectionExtension
{
    public static IServiceCollection AddDailyStatsServices(this IServiceCollection services)
    {
        services.AddAndValidateOptions<DailyStatsJobConfig>()
                .AddAndValidateOptions<DailyStatsConfig>();

        services.AddTransient<IDailyStatsPoster, DailyStatsPoster>()
                .AddTransient<IUserActivityAnalyser, WordCountUserActivityAnalyser>()
                .AddTransient<IDailyStatsChartProvider, DailyStatsChartProvider>()
                .AddTransient<IRecentMessagesFetcher, RecentMessagesFetcher>()
                .AddTransient<IMessageFormatter, MessageFormatter>();

        return services;
    }
}
