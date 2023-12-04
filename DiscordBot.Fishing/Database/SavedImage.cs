using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Database;

[Index(nameof(Name))]
internal class SavedImage
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public required Uri Uri { get; set; }
}
