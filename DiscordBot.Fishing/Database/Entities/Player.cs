using DiscordBot.Features.Fishing.Database.Entities.Equipment;

namespace DiscordBot.Features.Fishing.Database.Entities;

internal class Player
{
    public int Id { get; init; }

    public required ulong DiscordId { get; init; }

    public required string DiscordName { get; init; }

    public GameState? GameState { get; init; }

    public required ICollection<OwnedItem> OwnedItems { get; set; }
}
