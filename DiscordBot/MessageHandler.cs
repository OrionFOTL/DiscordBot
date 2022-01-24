using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Commands;

namespace DiscordBot;

public class MessageHandler
{
    private const char _commandPrefix = '$';

    private readonly ILogger<MessageHandler> _logger;
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _serviceProvider;

    public MessageHandler(ILogger<MessageHandler> logger, DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _client = client;
        _commands = commands;
        _serviceProvider = serviceProvider;
    }

    public async Task InstallCommandsAsync()
    {
        _commands.CommandExecuted += CommandExecuted;
        _client.MessageReceived += HandleMessageAsync;

        await _commands.AddModulesAsync(typeof(EggplantModule).Assembly, _serviceProvider);
    }

    private async Task CommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext commandContext, IResult result)
    {
        if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
        {
            _logger.LogError(result.ErrorReason);
        }

        await Task.CompletedTask;
    }

    private async Task HandleMessageAsync(SocketMessage message)
    {
        if (message is not SocketUserMessage userMessage || userMessage.Source != MessageSource.User)
        {
            return;
        }

        var commandStartPos = 0;
        if (!userMessage.HasCharPrefix(_commandPrefix, ref commandStartPos))
        {
            //return;
        }

        var commandContext = new SocketCommandContext(_client, userMessage);

        await _commands.ExecuteAsync(commandContext, commandStartPos, _serviceProvider);
    }
}
