using Discord.Interactions;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Database.Entities;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Stages.Equipment;

internal class EquipmentItemViewInteractionReceiver(
    DatabaseContext databaseContext,
    IStateHandlerFactory stateHandlerFactory) : InteractionReceiver(databaseContext, stateHandlerFactory)
{
    [ComponentInteraction(nameof(Trigger.EquipmentItemSelected))]
    public async Task EquipmentItemSelected(string[] selectedItemIds)
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        var selectedItem = await DatabaseContext
            .Entry(GameState.Player)
            .Collection(p => p.OwnedItems)
            .Query()
            .FirstOrDefaultAsync(oi => oi.Id == new Guid(selectedItemIds[0]))
            ?? throw new InvalidOperationException($"Player {GameState.Player.Id} doesn't own an item with id {selectedItemIds[0]}");

        GameState.ViewingOwnedItem = selectedItem;

        Fire(Trigger.EquipmentItemSelected);
    }
}
