using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Clients;
using DiscordBot.Clients.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiscordBot
{
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
            services.AddSingleton<DiscordSocketClient>()
                    .AddSingleton<MessageHandler>()
                    .AddSingleton<CommandService>()
                    .AddSingleton<IBooruClient, BooruClient>();
        }
    }
}
