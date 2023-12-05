using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Database;

[Index(nameof(Code), IsUnique = true)]
internal class Location
{
    public int Id { get; init; }

    public required string Code { get; init; }

    public required string Name { get; init; }
}
