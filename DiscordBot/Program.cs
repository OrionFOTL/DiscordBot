using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services;
using DiscordBot.Services.Interface;

namespace DiscordBot;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>()
                        .ConfigureBotServices();
            });

    private static void ConfigureBotServices(this IServiceCollection services)
    {
        DiscordSocketConfig clientConfig = new()
        {
            GatewayIntents = GatewayIntents.AllUnprivileged
                             & ~GatewayIntents.GuildScheduledEvents
                             & ~GatewayIntents.GuildInvites
        };

        services.AddSingleton<HttpClient>()
                .AddSingleton(new DiscordSocketClient(clientConfig))
                .AddSingleton<CommandService>()
                .AddSingleton<InteractionService>()
                .AddSingleton<IBooruClient, NewBooruClient>()
                .AddSingleton<ISauceClient, SauceClient>()
                .AddSingleton<ITagClient, GelbooruTagClient>();
    }
}
