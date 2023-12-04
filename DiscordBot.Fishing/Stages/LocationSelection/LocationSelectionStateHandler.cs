using Discord;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Embeds;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;

namespace DiscordBot.Features.Fishing.Stages.LocationSelection;

internal interface ILocationSelectionStateHandler : IStateHandler;

internal class LocationSelectionStateHandler(IEmbedFormatter embedFormatter) : ILocationSelectionStateHandler
{
    public async Task Handle(IDiscordInteraction interaction, GameState gameState)
    {
        var footer = await embedFormatter.GetStandardFooter();

        await interaction.ModifyOriginalResponseAsync(mp =>
        {
            embedFormatter.ClearMessage(mp);
            mp.Embed = new EmbedBuilder()
            {
                Title = "Map",
                Description = "Where will you go fishing today?",
                Footer = footer,
            }.Build();
            mp.Components = new ComponentBuilder()
                .WithButton("Japan", $"{nameof(Trigger.LocationSelected)}-jp", ButtonStyle.Secondary, emote: new Emoji("🌸"))
                .Build();
        });
    }
}
