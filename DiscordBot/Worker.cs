using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Commands;

namespace DiscordBot;

public class Worker : BackgroundService
{
    private readonly string _discordBotToken;

    private readonly ILogger<Worker> _logger;
    private readonly DiscordSocketClient _discordClient;
    private readonly CommandService _textCommandService;
    private readonly IServiceProvider _serviceProvider;

    public Worker(
        IConfiguration configuration,
        ILogger<Worker> logger,
        DiscordSocketClient discordClient,
        CommandService textCommandService,
        IServiceProvider serviceProvider)
    {
        _discordBotToken = configuration["DiscordBotToken"];

        _logger = logger;
        _discordClient = discordClient;
        _textCommandService = textCommandService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting discord bot");

        _discordClient.Log += Client_Log;
        _discordClient.MessageReceived += HandleMessageReceived;

        _textCommandService.CommandExecuted += CommandExecuted;

        await _textCommandService.AddModulesAsync(typeof(EggplantModule).Assembly, _serviceProvider);

        await _discordClient.LoginAsync(TokenType.Bot, _discordBotToken);
        await _discordClient.StartAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken) => await _discordClient.StopAsync();

    private Task Client_Log(LogMessage arg)
    {
        _logger.LogInformation(arg.ToString());
        return Task.CompletedTask;
    }

    private async Task HandleMessageReceived(SocketMessage socketMessage)
    {
        if (socketMessage is not SocketUserMessage userMessage || userMessage.Source != MessageSource.User)
        {
            return;
        }

        var commandStartPos = 0;
        if (!userMessage.HasCharPrefix('-', ref commandStartPos))
        {
            //return;
        }

        var commandContext = new SocketCommandContext(_discordClient, userMessage);

        await _textCommandService.ExecuteAsync(commandContext, commandStartPos, _serviceProvider);
    }

    private Task CommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext commandContext, Discord.Commands.IResult result)
    {
        if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
        {
            _logger.LogError(result.ErrorReason);
        }

        return Task.CompletedTask;
    }
}
