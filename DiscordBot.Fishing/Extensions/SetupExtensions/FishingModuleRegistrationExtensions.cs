using System.Reflection;
using Discord.Interactions;

namespace DiscordBot.Features.Fishing.Extensions.SetupExtensions;

public static class FishingModuleRegistrationExtensions
{
    public static async Task AddFishingGameInteractionModules(this InteractionService interactionService, IServiceProvider serviceProvider)
    {
        var interactionModules = Assembly.GetExecutingAssembly()
                                         .GetTypes()
                                         .Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(InteractionModuleBase)))
                                         .ToList();

        foreach (var interactionModule in interactionModules)
        {
            await interactionService.AddModuleAsync(interactionModule, serviceProvider);
        }
    }
}
