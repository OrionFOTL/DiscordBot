using Discord;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Database.Entities;
using DiscordBot.Features.Fishing.Database.Entities.Equipment;
using DiscordBot.Features.Fishing.Embeds;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Stages.Equipment;

internal interface IEquipmentItemViewStateHandler : IStateHandler;

internal class EquipmentItemViewStateHandler(DatabaseContext databaseContext, IEmbedFormatter embedFormatter) : IEquipmentItemViewStateHandler
{
    public async Task Handle(IDiscordInteraction interaction, GameState gameState)
    {
        var viewingOwnedItem = await databaseContext.Entry(gameState).Reference(s => s.ViewingOwnedItem).Query()
            .Include(oi => oi.Item)
            .SingleAsync();

        if (await databaseContext.Entry(gameState.Player).Collection(p => p.OwnedItems).Query().AnyAsync(oi => oi == viewingOwnedItem) == false)
        {
            throw new InvalidOperationException($"Trying to view item {viewingOwnedItem.Id} which doesn't belong to player {gameState.Player.Id}");
        }

        List<EmbedFieldBuilder> itemFields = [
            .. (viewingOwnedItem is IQuantifiable quantifiable
                ? [new EmbedFieldBuilder { Name = "Amount", Value = quantifiable.Amount, IsInline = true }]
                : Array.Empty<EmbedFieldBuilder>()),
            new EmbedFieldBuilder { Name = "Price", Value = viewingOwnedItem.Item.Price, IsInline = true }
        ];

        var footer = await embedFormatter.GetStandardFooter();

        await interaction.ModifyOriginalResponseAsync(mp =>
        {
            embedFormatter.ClearMessage(mp);
            mp.Embed = new EmbedBuilder()
            {
                Title = viewingOwnedItem.Item.Name,
                Description = viewingOwnedItem.Item.Description,
                Fields = itemFields,
                ImageUrl = "https://i.imgur.com/QtCpDqJ.png",
                Footer = footer,
            }.Build();
            mp.Components = new ComponentBuilder()
                .WithButton("Equip", nameof(Trigger.EquipItem), ButtonStyle.Secondary, new Emoji("🫳"), disabled: viewingOwnedItem.Equipped)
                .WithButton("Back to equipment", nameof(Trigger.ViewEquipment), ButtonStyle.Secondary)
                .Build();
        });
    }
}
