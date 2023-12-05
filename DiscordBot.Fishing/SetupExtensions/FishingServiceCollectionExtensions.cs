using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Embeds;
using DiscordBot.Features.Fishing.Services.StaticImages;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.Stages.Equipment;
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
        services.AddAndValidateOptions<ImageSaverSettings>();

        services.AddTransient<IStateHandlerFactory, StateHandlerFactory>();
        services.AddTransient<IStaticImageFetcher, StaticImageFetcher>();
        services.AddTransient<IEmbedFormatter, EmbedFormatter>();

        services.AddTransient<IMainMenuStateHandler, MainMenuStateHandler>();
        services.AddTransient<IMainEquipmentViewStateHandler, MainEquipmentViewStateHandler>();
        services.AddTransient<ILocationSelectionStateHandler, LocationSelectionStateHandler>();
        services.AddTransient<IOnLocationStateHandler, OnLocationStateHandler>();

        services.AddDbContext<DatabaseContext>(o => o.UseSqlite(configuration["DbConnectionString"]));

        return services;
    }
}
