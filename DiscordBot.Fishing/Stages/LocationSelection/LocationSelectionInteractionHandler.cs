using Discord.Interactions;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;

namespace DiscordBot.Features.Fishing.Stages.LocationSelection;

internal class LocationSelectionInteractionHandler(
    DatabaseContext databaseContext,
    IStateHandlerFactory stateHandlerFactory) : InteractionHandler(databaseContext, stateHandlerFactory)
{
    [ComponentInteraction("location-*")]
    public Task LocationSelected(string locationCode)
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        if (locationCode is not "jp")
        {
            throw new InvalidOperationException();
        }

        GameState.CurrentLocationCode = locationCode;
        GameState.Message = $"{locationCode} chosen!";

        Fire(Trigger.LocationSelected);

        return Task.CompletedTask;
    }
}
