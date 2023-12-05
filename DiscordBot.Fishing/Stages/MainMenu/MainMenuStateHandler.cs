using Discord;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Embeds;
using DiscordBot.Features.Fishing.Services.StaticImages;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;

namespace DiscordBot.Features.Fishing.Stages.MainMenu;

internal interface IMainMenuStateHandler : IStateHandler;

internal class MainMenuStateHandler(IStaticImageFetcher staticImageFetcher, IEmbedFormatter embedFormatter) : IMainMenuStateHandler
{
    public async Task Handle(IDiscordInteraction interaction, GameState gameState)
    {
        var welcomeScreenImageUri = await staticImageFetcher.GetImageUri(ResourceImage.WelcomeScreen);
        var footer = await embedFormatter.GetStandardFooter();

        await interaction.ModifyOriginalResponseAsync(mp =>
        {
            embedFormatter.ClearMessage(mp);
            mp.Embed = new EmbedBuilder()
            {
                Title = "Main menu",
                Description = $"Hello {gameState.Player.DiscordName}, welcome to Fishing Game!",
                ImageUrl = welcomeScreenImageUri,
                Footer = footer,
            }.Build();
            mp.Components = new ComponentBuilder()
                .WithButton("Go Fishing", nameof(Trigger.GoToLocationSelect), emote: new Emoji("🎣"))
                .WithButton("View your equipment", nameof(Trigger.ViewEquipment), ButtonStyle.Secondary, new Emoji("🛠️"))
                .Build();
        });
    }
}
