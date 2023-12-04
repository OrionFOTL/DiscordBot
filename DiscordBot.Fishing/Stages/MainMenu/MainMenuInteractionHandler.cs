using Discord.Interactions;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;

namespace DiscordBot.Features.Fishing.Stages.MainMenu;

internal class MainMenuInteractionHandler(
    DatabaseContext databaseContext,
    IStateHandlerFactory stateHandlerFactory) : InteractionHandler(databaseContext, stateHandlerFactory)
{
    [ComponentInteraction(nameof(Trigger.GoToMenu))]
    public Task BackToMenu()
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        Fire(Trigger.GoToMenu);

        return Task.CompletedTask;
    }
}
