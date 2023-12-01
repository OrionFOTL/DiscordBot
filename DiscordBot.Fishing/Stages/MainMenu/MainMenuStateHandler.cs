using Discord;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;

namespace DiscordBot.Features.Fishing.Stages.MainMenu;

internal interface IMainMenuStateHandler : IStateHandler;

internal class MainMenuStateHandler : IMainMenuStateHandler
{
    public async Task Handle(IDiscordInteraction interaction, GameState gameState)
    {
        var message = $"Hello {gameState.Player}, welcome to Fishing Game!";

        var buttons = new ComponentBuilder()
            .WithButton("Go Fishing", "go-fishing", emote: new Emoji("🎣"));

        await interaction.ModifyOriginalResponseAsync(mp =>
        {
            mp.Content = message;
            mp.Components = buttons.Build();
        });
    }
}
