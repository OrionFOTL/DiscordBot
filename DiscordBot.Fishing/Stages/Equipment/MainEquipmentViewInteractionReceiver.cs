using Discord.Interactions;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Extensions;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Stages.Equipment;

internal class MainEquipmentViewInteractionReceiver(
    DatabaseContext databaseContext,
    IStateHandlerFactory stateHandlerFactory) : InteractionReceiver(databaseContext, stateHandlerFactory)
{
    [ComponentInteraction(nameof(Trigger.ViewEquipment))]
    public Task ViewEquipment()
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        Fire(Trigger.ViewEquipment);

        return Task.CompletedTask;
    }

    [ComponentInteraction(nameof(Trigger.EquipItem))]
    public async Task EquipItem()
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        await DatabaseContext.Entry(GameState).Reference(s => s.ViewingOwnedItem).LoadAsync();

        InvalidOperationExceptionExtensions.ThrowIfNull(GameState.ViewingOwnedItem);

        var playerOwnedItems = DatabaseContext.Entry(GameState.Player).Collection(p => p.OwnedItems);

        if (await playerOwnedItems.Query().AnyAsync(oi => oi == GameState.ViewingOwnedItem) == false)
        {
            throw new InvalidOperationException($"The ownedItem id {GameState.ViewingOwnedItem.Id} chosen to equip does not belong to player {GameState.Player.Id}");
        }

        var ownedItemsGroup = await playerOwnedItems.Query().OfTypes(GameState.ViewingOwnedItem.GetType()).ToListAsync();

        foreach (var ownedItem in ownedItemsGroup)
        {
            ownedItem.Equipped = false;
        }

        GameState.ViewingOwnedItem.Equipped = true;

        Fire(Trigger.ViewEquipment);
    }
}
