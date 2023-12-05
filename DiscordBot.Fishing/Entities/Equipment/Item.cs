namespace DiscordBot.Features.Fishing.Entities.Equipment;

internal abstract class Item
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public required double Price { get; init; }
}