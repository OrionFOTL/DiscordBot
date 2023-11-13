using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Extensions;
using DiscordBot.Services.ArtGallery.Images;
using DiscordBot.Services.ArtGallery.Source;
using DiscordBot.Services.ArtGallery.Tags;
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
                        .AddBotServices();
            });

    private static IServiceCollection AddBotServices(this IServiceCollection services)
    {
        var discordSocketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged
                             & ~GatewayIntents.GuildScheduledEvents
                             & ~GatewayIntents.GuildInvites
        };

        services.AddHttpClient()
                .AddSingleton(new DiscordSocketClient(discordSocketConfig))
                .AddSingleton<InteractionService>()
                .AddTransient<IBooruClient, NewGelbooruClient>()
                .AddTransient<ISauceClient, SauceClient>()
                .AddTransient<ITagClient, GelbooruWebTagClient>();

        return services;
    }
}
