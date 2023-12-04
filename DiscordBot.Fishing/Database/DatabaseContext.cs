using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Database;

internal class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<Player> Players { get; init; }

    public DbSet<GameState> GameStates { get; init; }

    public DbSet<Location> Locations { get; init; }

    public DbSet<SavedImage> SavedImages { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>().HasData(
            new Location { Id = 1, Code = "jp", Name = "Japan" });
    }
}
