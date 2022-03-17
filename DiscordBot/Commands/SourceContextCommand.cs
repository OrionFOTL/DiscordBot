using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services.Interface;

namespace DiscordBot.Commands;

public class SourceContextCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ISauceClient _sauceClient;

    public SourceContextCommand(ISauceClient sauceClient)
    {
        _sauceClient = sauceClient;
    }

    [MessageCommand("Get image sauce")]
    public async Task GetSauce(SocketUserMessage message)
    {
        await DeferAsync();

        List<string> imageUrls = new();

        imageUrls.AddRange(message.Embeds
            .GroupBy(e => e.Url)
            .Select(eg =>
            {
                var firstEmbed = eg.FirstOrDefault();

                return EndsWithImageExtension(firstEmbed.Image.GetValueOrDefault().Url)
                    ? firstEmbed.Image.GetValueOrDefault().Url
                    : EndsWithImageExtension(firstEmbed.Url)
                        ? firstEmbed.Url
                        : EndsWithImageExtension(firstEmbed.Thumbnail?.Url)
                            ? firstEmbed.Thumbnail?.Url
                            : null;
            }));
        
        imageUrls.AddRange(message.Attachments.Where(a => EndsWithImageExtension(a.Url))
                                           .Select(a => a.Url));

        var urlsWithSauceTasks = imageUrls.Select(url => new { Url = url, Sauces = _sauceClient.GetSauce(url) }).ToList();
        await Task.WhenAll(urlsWithSauceTasks.Select(url => url.Sauces));

        var urlsWithSauce = await Task.WhenAll(urlsWithSauceTasks.Select(async url => new { Url = url.Url, Sauces = await url.Sauces }));

        var embeds = urlsWithSauce.Select(s => new EmbedBuilder()
        .WithImageUrl(s.Url)
        .WithFields(s.Sauces.Select(sauce =>
        {
            string postTitle = string.IsNullOrEmpty(sauce.Title) ? "Post" : sauce.Title;
            string artistLink = string.IsNullOrEmpty(sauce.ArtistId) ? null : @"https://www.pixiv.net/member.php?id=" + sauce.ArtistId;
            string authorline = string.IsNullOrEmpty(sauce.ArtistName) ? null : $" by [{sauce.ArtistName}]({artistLink})";

            return new EmbedFieldBuilder
            {
                Name = sauce.SiteName,
                Value = $"[{postTitle}]({sauce.SourcePostUrl})" + authorline,
                IsInline = true,
            };
        }))
        .WithFooter(new EmbedFooterBuilder { Text = $"Invoked by {Context.Interaction.User}" })
        .Build());

        await message.ReplyAsync(
            "Sauces:",
            allowedMentions: new AllowedMentions { MentionRepliedUser = false },
            embeds: embeds.ToArray());

        var deferMessage = await GetOriginalResponseAsync();
        await deferMessage.DeleteAsync();
    }

    private static bool EndsWithImageExtension(string url)
    {
        if (url is null)
        {
            return false;
        }

        return url.EndsWith(".jpg")
            || url.EndsWith(".jpeg")
            || url.EndsWith(".png")
            || url.EndsWith(".gif")
            || url.EndsWith(".webp");
    }
}
