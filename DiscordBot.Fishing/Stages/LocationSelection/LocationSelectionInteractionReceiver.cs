using Discord.Interactions;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;

namespace DiscordBot.Features.Fishing.Stages.LocationSelection;

internal class LocationSelectionInteractionReceiver(
    DatabaseContext databaseContext,
    IStateHandlerFactory stateHandlerFactory) : InteractionReceiver(databaseContext, stateHandlerFactory)
{
    [ComponentInteraction(nameof(Trigger.GoToLocationSelect))]
    public Task GoToLocationSelect()
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        Fire(Trigger.GoToLocationSelect);

        return Task.CompletedTask;
    }
}
