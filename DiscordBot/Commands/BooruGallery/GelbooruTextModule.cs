using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Model;
using DiscordBot.Services.Interface;

namespace DiscordBot.Commands.BooruGallery;

public class GelbooruTextModule : ModuleBase<SocketCommandContext>
{
    private readonly ILogger<GelbooruTextModule> _logger;
    private readonly IBooruClient _booruClient;
    private readonly ISauceClient _sauceClient;
    private readonly DiscordSocketClient _discordClient;

    public GelbooruTextModule(ILogger<GelbooruTextModule> logger, DiscordSocketClient discordClient, IBooruClient booruClient, ISauceClient sauceClient)
    {
        _logger = logger;
        _booruClient = booruClient;
        _sauceClient = sauceClient;
        _discordClient = discordClient;
    }
    

    [Command("random", RunMode = RunMode.Async)]
    public async Task StartRandomGallery(params string[] tags)
    {
        Task<IUserMessage> fetchingReplyTask = Context.Message.ReplyAsync(embed: new EmbedBuilder().WithDescription("Fetching...").Build(), allowedMentions: AllowedMentions.None);
        Task<IEnumerable<(string Tag, int Count)>> suggestTagsTask = _booruClient.GetSimilarTags(tags.First());

        bool allowNsfw = Context.Channel switch
        {
            ITextChannel textChannel => textChannel.IsNsfw,
            IDMChannel dmChannel => true,
            _ => false,
        };

        Post image = await _booruClient.GetRandomImageAsync(noVideo: true, allowNsfw, tags);

        IUserMessage fetchingReply = await fetchingReplyTask;
        IEnumerable<(string Tag, int Count)> suggestedTags = await suggestTagsTask;

        if (image is null)
        {
            await fetchingReply.ModifyAsync(m =>
            {
                m.Content = $"You might've meant: {string.Join(", ", suggestedTags.Select(t => $"`{t.Tag} ({t.Count})`"))}";
                m.Embed = new EmbedBuilder().WithDescription("No images found.").Build();
            });
            return;
        }

        await fetchingReply.ModifyAsync(m =>
        {
            m.Content = $"You might've meant: {string.Join(", ", suggestedTags.Select(t => $"`{t.Tag} ({t.Count})`"))}";
            m.Embed = new EmbedBuilder
            {
                Title = "Random image:",
                Url = image.PostUrl,
                ImageUrl = image.FileUrl.ToString(),
            }.Build();
            m.Components = new ComponentBuilder()
                .WithButton(customId: $"paginator previous 1", style: ButtonStyle.Secondary, emote: new Emoji("◀"), disabled: true)
                .WithButton(customId: $"sauce 1", style: ButtonStyle.Secondary, emote: new Emoji("🍝"), label: "Sauce")
                .WithButton(customId: $"paginator next 1", style: ButtonStyle.Secondary, emote: new Emoji("▶"))
                .Build();
        });
    }
}
