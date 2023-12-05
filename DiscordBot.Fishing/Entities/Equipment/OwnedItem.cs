using System.ComponentModel.DataAnnotations;
using DiscordBot.Features.Fishing.Database;

namespace DiscordBot.Features.Fishing.Entities.Equipment;

internal abstract class OwnedItem
{
    [Key]
    public Guid Id { get; init; }

    public required Item Item { get; init; }

    public required Player Player { get; init; }

    public bool Equipped { get; set; } = false;
}
