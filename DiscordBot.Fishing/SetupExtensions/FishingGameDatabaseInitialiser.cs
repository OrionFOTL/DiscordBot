using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Features.Fishing.SetupExtensions;

public class FishingGameDatabaseInitialiser
{
    public static void InitialiseDatabase(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        dbContext.Database.Migrate();

        AddOrUpdate(
            dbContext,
            [
                new FishingRod { Id = new Guid("68afc51d-97f3-4706-a5c7-b8c367113894"), Name = "Basic Rod", Price = 20 },
                new Bait { Id = new Guid("29f6e1dd-594c-46ff-9bed-b07f126fa407"), Name = "Basic bait", Price = 5 },
            ]);

        dbContext.SaveChanges();
    }

    private static void AddOrUpdate(DatabaseContext databaseContext, IEnumerable<Item> itemsToAdd)
    {
        foreach (var itemToAdd in itemsToAdd)
        {
            var existingItem = databaseContext.Items.Find(itemToAdd.Id);
            if (existingItem is null)
            {
                databaseContext.Add(itemToAdd);
            }
            else
            {
                databaseContext.Entry(existingItem).CurrentValues.SetValues(itemToAdd);
            }
        }
    }
}
