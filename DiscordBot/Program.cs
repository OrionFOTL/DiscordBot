using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services.Images;
using DiscordBot.Services.Interface;
using DiscordBot.Services.Source;
using DiscordBot.Services.Tags;
using Serilog;
using Serilog.Events;

namespace DiscordBot;

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .Enrich.FromLogContext()
               .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}")
               .WriteTo.File(
                    Path.Combine(Path.GetTempPath(), "discordLog", "discordBot.txt"),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day)
               .CreateLogger();

        try
        {
            Log.Information("Starting host");
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
            .UseSerilog()
            .ConfigureServices(services =>
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
                .AddSingleton<IBooruClient, NewGelbooruClient>()
                .AddSingleton<ISauceClient, SauceClient>()
                .AddSingleton<ITagClient, GelbooruWebTagClient>();
    }
}
