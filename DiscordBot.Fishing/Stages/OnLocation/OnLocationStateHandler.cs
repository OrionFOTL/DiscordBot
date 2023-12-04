using Discord;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Embeds;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;

namespace DiscordBot.Features.Fishing.Stages.OnLocation;

internal interface IOnLocationStateHandler : IStateHandler;

internal class OnLocationStateHandler(DatabaseContext databaseContext, IEmbedFormatter embedFormatter) : IOnLocationStateHandler
{
    public async Task Handle(IDiscordInteraction interaction, GameState gameState)
    {
        await databaseContext.Entry(gameState).Reference(g => g.Location).LoadAsync();

        InvalidOperationExceptionExtensions.ThrowIfNull(gameState.Location, nameof(gameState.Location));

        var footer = await embedFormatter.GetStandardFooter();

        await interaction.ModifyOriginalResponseAsync(mp =>
        {
            embedFormatter.ClearMessage(mp);
            mp.Embed = new EmbedBuilder()
            {
                Title = gameState.Location.Name,
                Description = $"You took a bike to {gameState.Location.Name}. Throw your line, review your equipment, or go back.",
                Footer = footer,
            }.Build();
            mp.Components = new ComponentBuilder()
                .WithButton("Back to location selection", nameof(Trigger.GoToLocationSelect), ButtonStyle.Secondary)
                .WithButton("Back to menu", nameof(Trigger.GoToMenu), ButtonStyle.Secondary)
                .Build();
        });
    }
}