using DiscordBot.Features.Fishing.Database.Entities;
using DiscordBot.Features.Fishing.Database.Entities.Equipment;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Database;

internal class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<Player> Players { get; init; }

    public DbSet<GameState> GameStates { get; init; }

    public DbSet<Location> Locations { get; init; }

    public DbSet<SavedImage> SavedImages { get; init; }

    public DbSet<Item> Items { get; init; }

    public DbSet<OwnedItem> OwnedItems { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>().HasData(
            new Location { Id = 1, Code = "jp", Name = "Japan" });

        modelBuilder.Entity<Item>().UseTpcMappingStrategy();
        modelBuilder.Entity<FishingRod>();
        modelBuilder.Entity<Bait>();

        modelBuilder.Entity<OwnedItem>().UseTpcMappingStrategy();
        modelBuilder.Entity<OwnedFishingRod>();
        modelBuilder.Entity<OwnedBait>();
    }
}
