using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Model;
using Microsoft.Extensions.Options;

namespace DiscordBot;

public class Worker : BackgroundService
{
    private readonly string _discordBotToken;
    private readonly List<GuildConfig> _guilds;

    private readonly ILogger<Worker> _logger;
    private readonly DiscordSocketClient _discordClient;
    private readonly CommandService _textCommandService;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    public Worker(
        ILogger<Worker> logger,
        IOptions<Tokens> tokens,
        IOptions<GuildConfigs> guildConfig,
        DiscordSocketClient discordClient,
        CommandService textCommandService,
        InteractionService interactionService,
        IServiceProvider serviceProvider)
    {
        _discordBotToken = tokens.Value.DiscordBotToken;
        _guilds = guildConfig.Value.Guilds;

        _logger = logger;
        _discordClient = discordClient;
        _textCommandService = textCommandService;
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting discord bot");

        _discordClient.Log += Client_Log;
        _discordClient.Ready += RegisterSlashCommandsToGuilds;
        _discordClient.MessageReceived += HandleMessageReceived;
        _discordClient.InteractionCreated += HandleInteractionCreated;

        _textCommandService.CommandExecuted += CommandExecuted;
        _interactionService.Log += Client_Log;

        using (var serviceScope = _serviceProvider.CreateScope())
        {
            await _textCommandService.AddModulesAsync(Assembly.GetExecutingAssembly(), serviceScope.ServiceProvider);
            await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), serviceScope.ServiceProvider);
        }

        await _discordClient.LoginAsync(TokenType.Bot, _discordBotToken);
        await _discordClient.StartAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken) => await _discordClient.StopAsync();

    private Task Client_Log(LogMessage arg)
    {
        _logger.LogInformation(arg.ToString());
        return Task.CompletedTask;
    }

    private async Task RegisterSlashCommandsToGuilds()
    {
        foreach (var guild in _guilds)
        {
            await _interactionService.RegisterCommandsToGuildAsync(guild.GuildId);
        }
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

    private async Task HandleInteractionCreated(SocketInteraction interaction)
    {
        switch (interaction)
        {
            case IComponentInteraction messageComponent:
                _logger.LogInformation(
                    "Interaction received from user: {user}, type: {interactionType}, buttonId: {buttonId}",
                    messageComponent.User,
                    messageComponent.Type,
                    messageComponent.Data.CustomId);
                break;
            case IAutocompleteInteraction autocompleteInteraction:
                _logger.LogInformation(
                    "Interaction received from user: {user}, type: {interactionType}, slash command: {command}, argument: {argument}",
                    autocompleteInteraction.User,
                    autocompleteInteraction.Type,
                    autocompleteInteraction.Data.CommandName,
                    autocompleteInteraction.Data.Current.Value);
                break;
            case ISlashCommandInteraction slashCommandInteraction:
                _logger.LogInformation(
                    "Interaction received from user: {user}, type: {interactionType}, slash command: {command}, arguments: {@arguments}",
                    slashCommandInteraction.User,
                    slashCommandInteraction.Type,
                    slashCommandInteraction.Data.Name,
                    slashCommandInteraction.Data.Options);
                break;
        }

        var interactionContext = new SocketInteractionContext(_discordClient, interaction);
        await _interactionService.ExecuteCommandAsync(interactionContext, _serviceProvider);
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
