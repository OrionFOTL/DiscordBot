using Discord;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;

namespace DiscordBot.Features.Fishing.Stages.OnLocation;

internal interface IOnLocationStateHandler : IStateHandler;

internal class OnLocationStateHandler : IOnLocationStateHandler
{
    public async Task Handle(IDiscordInteraction interaction, GameState gameState)
    {
        await interaction.ModifyOriginalResponseAsync(mp =>
        {
            mp.Content = gameState.Message + $"You're at beautiful {gameState.CurrentLocationCode}";
            mp.Components = new ComponentBuilder().WithButton("Back to location selection", "back-to-locations")
                .WithButton("Back to menu", "back-to-menu").Build();
        });
    }
}