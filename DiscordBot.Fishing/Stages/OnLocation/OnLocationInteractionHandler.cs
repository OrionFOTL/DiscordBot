using Discord.Interactions;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;

namespace DiscordBot.Features.Fishing.Stages.OnLocation;

internal class OnLocationInteractionHandler(
    DatabaseContext databaseContext,
    IStateHandlerFactory stateHandlerFactory) : InteractionHandler(databaseContext, stateHandlerFactory)
{
    [ComponentInteraction("back-to-locations")]
    public Task BackToLocationSelect()
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        GameState.Message = $"After choosing {GameState.CurrentLocationCode}, you went back to location select!";

        Fire(Trigger.BackToLocationSelect);

        return Task.CompletedTask;
    }

    [ComponentInteraction("back-to-menu")]
    public Task BackToMenu()
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        GameState.Message = $"After choosing {GameState.CurrentLocationCode}, you went back to menu!";

        Fire(Trigger.BackToMenu);

        return Task.CompletedTask;
    }
}
