namespace DiscordBot.Features.Fishing.Entities.Equipment;

internal class OwnedBait : OwnedItem, IQuantifiable
{
    public int Amount { get; set; }
}
