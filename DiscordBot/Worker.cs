using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _commandHandler;
        private readonly string _discordBotToken;

        public Worker(ILogger<Worker> logger, DiscordSocketClient client, CommandHandler commandHandler, IConfiguration configuration)
        {
            _logger = logger;
            _client = client;
            _commandHandler = commandHandler;
            _discordBotToken = configuration["DiscordBotToken"];
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting discord bot");

            _client.Log += Client_Log;

            await _client.LoginAsync(TokenType.Bot, _discordBotToken);
            await _commandHandler.InstallCommandsAsync();
            await _client.StartAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await Task.CompletedTask;

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping bot");
            await _client.LogoutAsync();
        }

        private async Task Client_Log(LogMessage arg)
        {
            _logger.LogInformation(arg.ToString());
            await Task.CompletedTask;
        }
    }
}
