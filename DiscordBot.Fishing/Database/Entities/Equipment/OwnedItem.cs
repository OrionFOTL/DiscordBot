using System.ComponentModel.DataAnnotations;

namespace DiscordBot.Features.Fishing.Database.Entities.Equipment;

internal abstract class OwnedItem
{
    [Key]
    public Guid Id { get; init; }

    public required Item Item { get; init; }

    public required Player Player { get; init; }

    public bool Equipped { get; set; } = false;
}
