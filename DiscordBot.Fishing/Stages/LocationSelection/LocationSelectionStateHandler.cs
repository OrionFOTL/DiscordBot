using Discord;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;

namespace DiscordBot.Features.Fishing.Stages.LocationSelection;

internal interface ILocationSelectionStateHandler : IStateHandler;

internal class LocationSelectionStateHandler : ILocationSelectionStateHandler
{
    public async Task Handle(IDiscordInteraction interaction, GameState gameState)
    {
        await interaction.ModifyOriginalResponseAsync(mp =>
        {
            mp.Content = "Choose location to go to!";
            mp.Components = new ComponentBuilder().WithButton("Japan!", "location-jp", ButtonStyle.Secondary).Build();
        });
    }
}
