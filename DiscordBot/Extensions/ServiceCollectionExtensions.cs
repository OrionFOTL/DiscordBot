using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Features.DailyStats.Job;
using DiscordBot.Features.DailyStats.ServiceCollectionExtension;
using DiscordBot.Features.Fishing.SetupExtensions;
using DiscordBot.Services.ArtGallery.Images;
using DiscordBot.Services.ArtGallery.Source;
using DiscordBot.Services.ArtGallery.Tags;
using Quartz;

namespace DiscordBot.Extensions;

internal static class ServiceCollectionExtensions
{

    public static IServiceCollection AddBotServices(this IServiceCollection services, IConfiguration configuration)
    {
        var discordSocketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged
                             & ~GatewayIntents.GuildScheduledEvents
                             & ~GatewayIntents.GuildInvites
        };

        var discordSocketClient = new DiscordSocketClient(discordSocketConfig);

        services.AddHttpClient()
                .AddSingleton(discordSocketClient)
                .AddSingleton<IDiscordClient>(discordSocketClient)
                .AddSingleton<InteractionService>()
                .AddTransient<IBooruClient, NewGelbooruClient>()
                .AddTransient<ISauceClient, SauceClient>()
                .AddTransient<ITagClient, GelbooruWebTagClient>()
                .AddDailyStatsServices()
                .AddFishingGameServices(configuration)
                .AddQuartz(o =>
                    o.ScheduleJob<DailyStatsJob>(trigger => trigger
                        .WithIdentity(nameof(DailyStatsJob))
                        .WithCronSchedule(configuration["DailyStatsJobConfig:Cron"] ?? throw new Exception("DailyStatsJobConfig:Cron was null or not found"))))
                .AddQuartzHostedService();

        return services;
    }
}