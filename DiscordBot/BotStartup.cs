using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Extensions;
using DiscordBot.Features;
using DiscordBot.Features.Fishing.SetupExtensions;
using Microsoft.Extensions.Options;

namespace DiscordBot;

internal class BotStartup(
    ILogger<BotStartup> logger,
    IOptions<BotConfig> botConfig,
    DiscordSocketClient discordClient,
    InteractionService interactionService,
    IServiceProvider serviceProvider) : BackgroundService
{
    private readonly string _discordBotToken = botConfig.Value.BotToken;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting discord bot");

        discordClient.Log += Log;
        discordClient.Ready += RegisterSlashCommandsToGuilds;
        discordClient.InteractionCreated += HandleInteraction;
        interactionService.Log += Log;

        using (var scope = serviceProvider.CreateScope())
        {
            await interactionService.AddModulesAsync(typeof(InteractionTestsModule).Assembly, scope.ServiceProvider);
            await interactionService.AddFishingGameInteractionModules(scope.ServiceProvider);
        }

        await discordClient.LoginAsync(TokenType.Bot, _discordBotToken);
        await discordClient.StartAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken) => await discordClient.StopAsync();

    private Task Log(LogMessage log)
    {
        using var scope = logger.BeginScope(log);

        logger.Log(log.Severity.ToLogLevel(), log.Exception, "{message}", log.Message);
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
            await interactionService.RegisterCommandsToGuildAsync(guildId);
        }
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        switch (interaction)
        {
            case IComponentInteraction messageComponent:
                logger.LogInformation(
                    "Interaction received from user: {user}, type: {interactionType}, buttonId: {buttonId}",
                    messageComponent.User,
                    messageComponent.Type,
                    messageComponent.Data.CustomId);
                break;
            case IAutocompleteInteraction autocompleteInteraction:
                logger.LogInformation(
                    "Interaction received from user: {user}, type: {interactionType}, slash command: {command}, argument: {argument}",
                    autocompleteInteraction.User,
                    autocompleteInteraction.Type,
                    autocompleteInteraction.Data.CommandName,
                    autocompleteInteraction.Data.Current.Value);
                break;
            case ISlashCommandInteraction slashCommandInteraction:
                logger.LogInformation(
                    "Interaction received from user: {user}, type: {interactionType}, slash command: {command}, arguments: {@arguments}",
                    slashCommandInteraction.User,
                    slashCommandInteraction.Type,
                    slashCommandInteraction.Data.Name,
                    slashCommandInteraction.Data.Options);
                break;
        }

        var interactionContext = new SocketInteractionContext(discordClient, interaction);
        await interactionService.ExecuteCommandAsync(interactionContext, serviceProvider);
    }
}
