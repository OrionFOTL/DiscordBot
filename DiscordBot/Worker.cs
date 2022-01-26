using Discord;
using Discord.WebSocket;

namespace DiscordBot;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly DiscordSocketClient _client;
    private readonly MessageHandler _messageHandler;
    private readonly string _discordBotToken;

    public Worker(ILogger<Worker> logger, DiscordSocketClient client, MessageHandler messageHandler, IConfiguration configuration)
    {
        _logger = logger;
        _client = client;
        _messageHandler = messageHandler;
        _discordBotToken = configuration["DiscordBotToken"];
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting discord bot");

        _client.Log += Client_Log;

        await _client.LoginAsync(TokenType.Bot, _discordBotToken);
        await _messageHandler.InstallCommandsAsync();
        await _client.StartAsync();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping bot");
        await _client.StopAsync();
    }

    private Task Client_Log(LogMessage arg)
    {
        _logger.LogInformation(arg.ToString());
        return Task.CompletedTask;
    }
}
