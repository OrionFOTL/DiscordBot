using DiscordBot.Features.Fishing.Database.Entities;
using DiscordBot.Features.Fishing.Database.Entities.Equipment;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Database.Repositories;

internal interface IPlayerOwnedItemsRepository
{
    Task<List<IGrouping<Type, OwnedItem>>> GetPlayerOwnedItemsGroupedByType(Player player);
}

internal class PlayerOwnedItemsRepository(DatabaseContext databaseContext) : IPlayerOwnedItemsRepository
{
    public async Task<List<IGrouping<Type, OwnedItem>>> GetPlayerOwnedItemsGroupedByType(Player player)
    {
        var yourEquipment = (await databaseContext
            .Entry(player)
            .Collection(p => p.OwnedItems)
            .Query()
            .Include(ownedItem => ownedItem.Item)
            .OrderByDescending(ownedItem => ownedItem is OwnedFishingRod)
            .ThenByDescending(ownedItem => ownedItem is OwnedBait)
            .ToListAsync())
            .GroupBy(ownedItem => ownedItem.GetType())
            .ToList();

        return yourEquipment;
    }
}
