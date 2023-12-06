namespace DiscordBot.Features.Fishing.Database.Entities.Equipment;

internal abstract class Item
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required double Price { get; init; }
}