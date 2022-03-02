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
        private readonly ITagClient _tagClient;
        private readonly ISauceClient _sauceClient;

        public GelbooruInteractionModule(
            ILogger<GelbooruInteractionModule> logger,
            IBooruClient booruClient,
            ITagClient tagClient,
            ISauceClient sauceClient)
        {
            _logger = logger;
            _booruClient = booruClient;
            _tagClient = tagClient;
            _sauceClient = sauceClient;
        }

        [SlashCommand("random", "Start a gallery", runMode: RunMode.Async)]
        public async Task StartRandomGallery(
            [Autocomplete(typeof(TagAutocompleteHandler))] string tag1,
            [Autocomplete(typeof(TagAutocompleteHandler))] string tag2 = null,
            [Autocomplete(typeof(TagAutocompleteHandler))] string tag3 = null)
        {
            var tags = new[] { tag1, tag2, tag3 }.Where(t => t is not null).Select(t => t.Trim()).ToArray();

            Task fetchingReplyTask = Context.Interaction.RespondAsync(embed: new EmbedBuilder().WithDescription("Fetching...").Build(), allowedMentions: AllowedMentions.None);
            Task<IEnumerable<(string Tag, int Count)>> suggestTagsTask = _tagClient.GetSimilarTags(tags.First());

            bool allowNsfw = Context.Channel switch
            {
                ITextChannel textChannel => textChannel.IsNsfw,
                IDMChannel dmChannel => true,
                _ => false,
            };

            Post image = await _booruClient.GetRandomImageAsync(noVideo: true, allowNsfw, tags);
            await fetchingReplyTask;

            IEnumerable<(string Tag, int Count)> suggestedTags = await suggestTagsTask;

            if (image is null)
            {
                await ModifyOriginalResponseAsync(m =>
                {
                    m.Content = $"Gallery of {string.Join(", ", tags.Select(t => $"`{t}`"))}";
                    m.Embed = new EmbedBuilder().WithDescription("No images found.").Build();
                });
                return;
            }

            string joinedTags = string.Join(';', tags);
            await ModifyOriginalResponseAsync(m =>
            {
                m.Content = $"Gallery of {string.Join(", ", tags.Select(t => $"`{t}`"))}";
                m.Embed = new EmbedBuilder
                {
                    Title = "Random image:",
                    Url = image.PostUrl,
                    ImageUrl = image.FileUrl.ToString(),
                }.Build();
                m.Components = new ComponentBuilder()
                    .WithButton(customId: $"go:p,1,{joinedTags}", style: ButtonStyle.Secondary, emote: new Emoji("◀"), disabled: true)
                    .WithButton(customId: $"sauce", style: ButtonStyle.Secondary, emote: new Emoji("🍝"), label: "Sauce")
                    .WithButton(customId: $"go:n,1,{joinedTags}", style: ButtonStyle.Secondary, emote: new Emoji("▶"))
                    .Build();
            });
        }

        [ComponentInteraction("go:*,*,*")]
        public async Task ChangePage(string direction, string currentPage, string longTags)
        {
            var interaction = (SocketMessageComponent)Context.Interaction;

            IEnumerable<ButtonComponent> messageButtons = interaction.Message.Components.First().Components.OfType<ButtonComponent>();
            IEnumerable<IMessageComponent> disabledButtons = messageButtons.Select<ButtonComponent, IMessageComponent>(button => button.ToBuilder().WithDisabled(true).Build());

            Task loadingMessageTask = interaction.UpdateAsync(mp =>
            {
                mp.Embed = interaction.Message.Embeds.First()
                    .ToEmbedBuilder()
                    .WithTitle($"Loading {(direction == "n" ? "next" : "previous")} image...")
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

            var tags = longTags.Split(';');
            int requestedPage = int.Parse(currentPage) + (direction == "n" ? 1 : -1);
            var updatedImage = await _booruClient.GetRandomImageAsync(noVideo: true, allowNsfw: allowNsfw, contentTags: tags.ToArray());

            await loadingMessageTask;

            if (updatedImage is null)
            {
                await interaction.ModifyOriginalResponseAsync(mp =>
                {
                    mp.Embed = new EmbedBuilder().WithTitle("No further images found.").Build();
                    mp.Components = new ComponentBuilder()
                        .WithButton(customId: $"go:p,{requestedPage},{longTags}", style: ButtonStyle.Secondary, emote: new Emoji("◀"), disabled: requestedPage <= 1)
                        .WithButton(customId: "sauce", style: ButtonStyle.Secondary, emote: new Emoji("🍝"), label: "Sauce", disabled: true)
                        .WithButton(customId: $"go:n,{requestedPage},{longTags}", style: ButtonStyle.Secondary, emote: new Emoji("▶"), disabled: direction == "n")
                        .Build();
                });
                return;
            }

            await interaction.ModifyOriginalResponseAsync(mp =>
            {
                mp.Embed = new EmbedBuilder
                {
                    Title = $"Random image #{requestedPage}:",
                    Url = updatedImage.PostUrl,
                    ImageUrl = updatedImage.FileUrl.ToString(),
                }.Build();
                mp.Components = new ComponentBuilder()
                    .WithButton(customId: $"go:p,{requestedPage},{longTags}", style: ButtonStyle.Secondary, emote: new Emoji("◀"), disabled: requestedPage <= 1)
                    .WithButton(customId: $"sauce", style: ButtonStyle.Secondary, emote: new Emoji("🍝"), label: "Sauce")
                    .WithButton(customId: $"go:n,{requestedPage},{longTags}", style: ButtonStyle.Secondary, emote: new Emoji("▶"))
                    .Build();
            });
        }

        [ComponentInteraction("sauce")]
        public async Task AddSauce()
        {
            var interaction = (SocketMessageComponent)Context.Interaction;

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
                mp.Components = new ComponentBuilder().AddRow(new ActionRowBuilder().WithComponents(galleryButtons))
                                                      .Build();
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

            await interaction.ModifyOriginalResponseAsync(mp =>
            {
                mp.Embed = interaction.Message.Embeds.First()
                    .ToEmbedBuilder()
                    .WithDescription(saucesFields.Any() ? "Sauce:" : "No sauces found")
                    .WithFields(saucesFields)
                    .Build();
            });
        }
    }
}
