using Discord.Interactions;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;

namespace DiscordBot.Features.Fishing.Stages.OnLocation;

internal class OnLocationInteractionReceiver(
    DatabaseContext databaseContext,
    IStateHandlerFactory stateHandlerFactory) : InteractionReceiver(databaseContext, stateHandlerFactory)
{
    [ComponentInteraction($"{nameof(Trigger.LocationSelected)}-*")]
    public Task LocationSelected(string locationCode)
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        var selectedLocation = DatabaseContext.Locations.FirstOrDefault(l => l.Code == locationCode)
            ?? throw new InvalidOperationException($"No location with code {locationCode} found");

        GameState.Location = selectedLocation;

        Fire(Trigger.LocationSelected);

        return Task.CompletedTask;
    }
}
