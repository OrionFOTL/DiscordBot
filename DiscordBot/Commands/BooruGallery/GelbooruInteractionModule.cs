using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Model;
using DiscordBot.Services.Interface;

namespace DiscordBot.Commands.BooruGallery
{
    public class GelbooruInteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ILogger<GelbooruInteractionModule> _logger;
        private readonly IBooruClient _booruClient;
        private readonly ISauceClient _sauceClient;

        public GelbooruInteractionModule(ILogger<GelbooruInteractionModule> logger, IBooruClient booruClient, ISauceClient sauceClient)
        {
            _logger = logger;
            _booruClient = booruClient;
            _sauceClient = sauceClient;
        }

        [ComponentInteraction("sauce")]
        public async Task AddSauce()
        {
            if (Context.Interaction is not SocketMessageComponent interaction)
            {
                await DeferAsync();
                return;
            }

            // disable Sauce button
            List<IMessageComponent> galleryButtons = interaction.Message.Components.First().Components.ToList();

            ButtonComponent sauceButton = galleryButtons.OfType<ButtonComponent>().First(c => c.CustomId == "sauce");
            ButtonComponent disabledSauceButton = sauceButton.ToBuilder().WithDisabled(true).Build();

            var sauceButtonIndex = galleryButtons.FindIndex(c => c == sauceButton);
            galleryButtons[sauceButtonIndex] = disabledSauceButton;

            Task fetchingMessageTask = interaction.UpdateAsync(mp =>
            {
                mp.Embed = interaction.Message.Embeds.First()
                    .ToEmbedBuilder()
                    .WithDescription("Fetching sauce... ⏳")
                    .Build();
                mp.Components = new ComponentBuilder().AddRow(new ActionRowBuilder().WithComponents(galleryButtons)).Build();
            });

            IEnumerable<SauceData> sauces = await _sauceClient.GetSauce(interaction.Message.Embeds.First().Image.Value.Url);

            var saucesFields = sauces.Select(s =>
            {
                string postTitle = string.IsNullOrEmpty(s.Title) ? "Post" : s.Title;
                string artistLink = string.IsNullOrEmpty(s.ArtistId) ? null : @"https://www.pixiv.net/member.php?id=" + s.ArtistId;
                string authorline = string.IsNullOrEmpty(s.ArtistName) ? null : $" by [{s.ArtistName}]({artistLink})";

                return new EmbedFieldBuilder
                {
                    Name = s.SiteName,
                    Value = $"[{postTitle}]({s.SourcePostUrl})" + authorline,
                    IsInline = true,
                };
            });

            await interaction.UpdateAsync(mp =>
            {
                mp.Embed = interaction.Message.Embeds.First()
                    .ToEmbedBuilder()
                    .WithDescription(saucesFields.Any() ? "Sauce:" : "No sauces found")
                    .WithFields(saucesFields)
                    .Build();
            });
        }

        [ComponentInteraction("paginator")]
        public async Task ChangePage()
        {
            if (Context.Interaction is not SocketMessageComponent interaction)
            {
                await DeferAsync();
                return;
            }

            string[] customValues = interaction.Data.CustomId.Split(' ');

            if (interaction.User.Id != interaction.Message.ReferencedMessage.Author.Id)
            {
                await FollowupAsync("Only the original user can browse the gallery.", ephemeral: true);
                return;
            }

            bool forward = customValues.Contains("next");

            IEnumerable<ButtonComponent> messageButtons = interaction.Message.Components.First().Components.OfType<ButtonComponent>();
            IEnumerable<IMessageComponent> disabledButtons = messageButtons.Select<ButtonComponent, IMessageComponent>(button => button.ToBuilder().WithDisabled(true).Build());

            Task loadingMessageTask = interaction.UpdateAsync(mp =>
            {
                mp.Embed = interaction.Message.Embeds.First()
                    .ToEmbedBuilder()
                    .WithTitle($"Loading {(forward ? "next" : "previous")} one...")
                    .WithUrl(null)
                    .Build();
                mp.Components = new ComponentBuilder().AddRow(new ActionRowBuilder().WithComponents(disabledButtons.ToList())).Build();
            });

            bool allowNsfw = Context.Channel switch
            {
                ITextChannel textChannel => textChannel.IsNsfw,
                IDMChannel => true,
                _ => false,
            };

            var tags = interaction.Message.ReferencedMessage.Content.Split(' ').Skip(1);

            int requestedPage = Convert.ToInt32(customValues.Last()) + (forward ? 1 : -1);

            var updatedImage = await _booruClient.GetRandomImageAsync(noVideo: true, allowNsfw: allowNsfw, contentTags: tags.ToArray());
            await loadingMessageTask;

            if (updatedImage is null)
            {
                await interaction.UpdateAsync(mp =>
                {
                    mp.Embed = new EmbedBuilder().WithTitle("No further images found.").Build();
                    mp.Components = new ComponentBuilder()
                        .WithButton(customId: $"paginator previous {requestedPage}", style: ButtonStyle.Secondary, emote: new Emoji("◀"), disabled: requestedPage <= 1)
                        .WithButton(customId: "sauce", style: ButtonStyle.Secondary, emote: new Emoji("🍝"), label: "Sauce", disabled: true)
                        .WithButton(customId: $"paginator next {requestedPage}", style: ButtonStyle.Secondary, emote: new Emoji("▶"), disabled: forward)
                        .Build();
                });
                return;
            }

            await interaction.UpdateAsync(mp =>
            {
                mp.Embed = new EmbedBuilder
                {
                    Title = $"Random image #{requestedPage}:",
                    Url = updatedImage.PostUrl,
                    ImageUrl = updatedImage.FileUrl.ToString(),
                }.Build();
                mp.Components = new ComponentBuilder()
                    .WithButton(customId: $"paginator previous {requestedPage}", style: ButtonStyle.Secondary, emote: new Emoji("◀"), disabled: requestedPage <= 1)
                    .WithButton(customId: $"sauce {requestedPage}", style: ButtonStyle.Secondary, emote: new Emoji("🍝"), label: "Sauce")
                    .WithButton(customId: $"paginator next {requestedPage}", style: ButtonStyle.Secondary, emote: new Emoji("▶"))
                    .Build();
            });
        }
    }
}
