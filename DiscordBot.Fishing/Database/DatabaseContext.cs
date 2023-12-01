using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Database;

internal class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; init; }

    public DbSet<GameState> GameStates { get; init; }


}
