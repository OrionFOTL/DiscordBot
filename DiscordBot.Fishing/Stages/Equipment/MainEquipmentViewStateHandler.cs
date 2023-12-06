using System.Text;
using Discord;
using DiscordBot.Features.Fishing.Database.Entities;
using DiscordBot.Features.Fishing.Database.Entities.Equipment;
using DiscordBot.Features.Fishing.Database.Repositories;
using DiscordBot.Features.Fishing.Embeds;
using DiscordBot.Features.Fishing.Services.StaticImages;
using DiscordBot.Features.Fishing.Stages.Base;
using DiscordBot.Features.Fishing.State;

namespace DiscordBot.Features.Fishing.Stages.Equipment;

internal interface IMainEquipmentViewStateHandler : IStateHandler;

internal class MainEquipmentViewStateHandler(
    IPlayerOwnedItemsRepository playerOwnedItemsRepository,
    IStaticImageFetcher staticImageFetcher,
    IEmbedFormatter embedFormatter) : IMainEquipmentViewStateHandler
{
    public async Task Handle(IDiscordInteraction interaction, GameState gameState)
    {
        var yourEquipment = await playerOwnedItemsRepository.GetPlayerOwnedItemsGroupedByType(gameState.Player);

        var itemSelectMenuOptions = yourEquipment.SelectMany(grp => grp).Select(ownedItem => new SelectMenuOptionBuilder
        {
            Description = ownedItem is IQuantifiable quantifiable ? $"Amount: {quantifiable.Amount}" : null,
            Emote = ownedItem switch
            {
                OwnedFishingRod => new Emoji("🎣"),
                OwnedBait => new Emoji("🪱"),
                { } x => new Emoji("🛠️"),
            },
            IsDefault = false,
            Label = ownedItem.Item.Name + (ownedItem.Equipped ? " (equipped)" : string.Empty),
            Value = ownedItem.Id.ToString(),
        }).ToList();

        var selectMenu = new SelectMenuBuilder
        {
            CustomId = nameof(Trigger.EquipmentItemSelected),
            Placeholder = "Select the item you'd like to inspect.",
            Options = itemSelectMenuOptions,
        };

        var fields = from grp in yourEquipment
                     let items = grp.Select(OwnedItemToListString)
                     select new EmbedFieldBuilder
                     {
                         Name = grp.First() switch
                         {
                             OwnedFishingRod => $"{new Emoji("🎣")} Fishing Rods",
                             OwnedBait => $"{new Emoji("🪱")} Bait",
                             { } x => x.Item.Name,
                         },
                         Value = string.Join(Environment.NewLine, items),
                     };

        var inventoryImage = await staticImageFetcher.GetImageUri(ResourceImage.EquipmentScreen);
        var footer = await embedFormatter.GetStandardFooter();

        await interaction.ModifyOriginalResponseAsync(mp =>
        {
            embedFormatter.ClearMessage(mp);
            mp.Embed = new EmbedBuilder()
            {
                Title = "Your equipment",
                Description = "Here's where you can browse your fishing equipment and choose items to equip.",
                Fields = fields.ToList(),
                ImageUrl = inventoryImage,
                Footer = footer,
            }.Build();
            mp.Components = new ComponentBuilder()
                .WithSelectMenu(selectMenu)
                .WithButton("Back to menu", nameof(Trigger.GoToMenu), ButtonStyle.Secondary)
                .Build();
        });
    }

    private static string OwnedItemToListString(OwnedItem ownedItem, int index)
    {
        var sb = new StringBuilder();

        sb.Append($"{index}. ");
        sb.Append(ownedItem.Item.Name);

        if (ownedItem is IQuantifiable quantifiable)
        {
            sb.Append($" x{quantifiable.Amount}");
        }

        if (ownedItem.Equipped)
        {
            sb.Append($" {Format.Italics("(equipped)")}");
        }

        return sb.ToString();
    }
}
