using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.Stages.LocationSelection;
using DiscordBot.Features.Fishing.Stages.MainMenu;
using DiscordBot.Features.Fishing.Stages.OnLocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Features.Fishing.SetupExtensions;

public static class FishingServiceCollectionExtensions
{
    public static IServiceCollection AddFishingGameServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IStateHandlerFactory, StateHandlerFactory>();

        services.AddTransient<IOnLocationStateHandler, OnLocationStateHandler>();
        services.AddTransient<ILocationSelectionStateHandler, LocationSelectionStateHandler>();
        services.AddTransient<IMainMenuStateHandler, MainMenuStateHandler>();

        services.AddDbContext<DatabaseContext>(o => o.UseSqlite(configuration["DbConnectionString"]));

        return services;
    }
}
