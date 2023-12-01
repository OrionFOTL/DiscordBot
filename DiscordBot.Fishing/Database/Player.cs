namespace DiscordBot.Features.Fishing.Database;

internal class Player
{
    public int Id { get; init; }

    public required ulong DiscordId { get; init; }

    public required string DiscordName { get; init; }

    public GameState? GameState { get; init; }
}
