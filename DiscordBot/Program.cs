using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Extensions;
using DiscordBot.Features.DailyStats;
using DiscordBot.Features.DailyStats.Charting;
using DiscordBot.Services.ArtGallery.Images;
using DiscordBot.Services.ArtGallery.Source;
using DiscordBot.Services.ArtGallery.Tags;
using Quartz;
using Serilog;

namespace DiscordBot;

internal static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("fatalLog.txt")
            .CreateBootstrapLogger();

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .UseSerilog((builder, config) => config.ReadFrom.Configuration(builder.Configuration))
            .ConfigureServices((builder, services) =>
            {
                services.AddHostedService<BotStartup>()
                        .AddAndValidateOptions<BotConfig>()
                        .AddAndValidateOptions<SaucenaoConfig>()
                        .AddAndValidateOptions<DailyStatsConfig>("DailyStats")
                        .AddAndValidateOptions<QuartzOptions>("Quartz")
                        .AddBotServices(builder.Configuration);
            });

    private static IServiceCollection AddBotServices(this IServiceCollection services, IConfiguration configuration)
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
                .AddTransient<IDailyStatsChartProvider, DailyStatsChartProvider>()
                .AddQuartz(o => 
                    o.ScheduleJob<DailyStatsJob>(trigger => trigger
                        .WithIdentity(nameof(DailyStatsJob))
                        .StartNow()
                        .WithCronSchedule(configuration["DailyStats:Cron"])))
                .AddQuartzHostedService();

        return services;
    }
}
