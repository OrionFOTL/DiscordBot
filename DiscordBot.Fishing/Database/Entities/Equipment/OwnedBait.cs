namespace DiscordBot.Features.Fishing.Database.Entities.Equipment;

internal class OwnedBait : OwnedItem, IQuantifiable
{
    public int Amount { get; set; }
}
