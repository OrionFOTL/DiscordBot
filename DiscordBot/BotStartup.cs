using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Commands;
using DiscordBot.Extensions;
using Microsoft.Extensions.Options;

namespace DiscordBot;

internal class BotStartup : BackgroundService
{
    private readonly string _discordBotToken;

    private readonly ILogger<BotStartup> _logger;
    private readonly DiscordSocketClient _discordClient;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    public BotStartup(
        ILogger<BotStartup> logger,
        IOptions<BotConfig> botConfig,
        DiscordSocketClient discordClient,
        InteractionService interactionService,
        IServiceProvider serviceProvider)
    {
        _discordBotToken = botConfig.Value.BotToken;

        _logger = logger;
        _discordClient = discordClient;
        _interactionService = interactionService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting discord bot");

        _discordClient.Log += Log;
        _discordClient.Ready += RegisterSlashCommandsToGuilds;
        _discordClient.InteractionCreated += HandleInteraction;

        _interactionService.Log += Log;

        using (var serviceScope = _serviceProvider.CreateScope())
        {
            await _interactionService.AddModulesAsync(typeof(InteractionTestsModule).Assembly, serviceScope.ServiceProvider);
        }

        await _discordClient.LoginAsync(TokenType.Bot, _discordBotToken);
        await _discordClient.StartAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken) => await _discordClient.StopAsync();

    private Task Log(LogMessage log)
    {
        using var scope = _logger.BeginScope(log);
        _logger.Log(log.Severity.ToLogLevel(), "{message}", log);
        return Task.CompletedTask;
    }

    private async Task RegisterSlashCommandsToGuilds()
    {
        var guildIds = new ulong[]
        {
            465358439313309696, // MML
            469986835641270282, // Jojo
            887064391445512294  // Orion
        };

        foreach (var guildId in guildIds)
        {
            await _interactionService.RegisterCommandsToGuildAsync(guildId);
        }
    }

    private async Task HandleInteraction(SocketInteraction interaction)
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
}
