using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services.ArtGallery.Source;

namespace DiscordBot.Features.Sauce;

public class SourceContextCommand(ISauceClient sauceClient) : InteractionModuleBase<SocketInteractionContext>
{
    [MessageCommand("Get image sauce")]
    public async Task GetSauce(SocketUserMessage message)
    {
        await DeferAsync();
        List<string> imageUrls = ExtractImageUrlsFromMessage(message);

        var urlsWithSauce = await sauceClient.GetSauce(imageUrls);

        var embeds = urlsWithSauce.Select(s => MakeEmbedFromSauces(s.Sauces)
            .WithImageUrl(s.Url)
            .WithFooter(new EmbedFooterBuilder { Text = $"Invoked by {Context.Interaction.User}" })
            .Build());

        await message.ReplyAsync(
            allowedMentions: new AllowedMentions { MentionRepliedUser = false },
            embeds: embeds.ToArray());

        var deferMessage = await GetOriginalResponseAsync();
        await deferMessage.DeleteAsync();
    }

    public static EmbedBuilder MakeEmbedFromSauces(IEnumerable<SauceData> sauces)
    {
        return new EmbedBuilder()
            .WithDescription(sauces.Any() ? "🍝 Sauces:" : "No sauces found.")
            .WithFields(sauces.Select(sauce => new EmbedFieldBuilder
            {
                Name = sauce.SiteName,
                Value = string.Join(' ', sauce.LinkedPost, sauce.Byline),
                IsInline = true,
            }));
    }

    public static List<string> ExtractImageUrlsFromMessage(IUserMessage message)
    {
        List<string> imageUrls =
        [
            .. message.Embeds
                .GroupBy(e => e.Url)
                .Select(eg =>
                {
                    var firstEmbed = eg.FirstOrDefault();

                    return firstEmbed is null
                        ? null
                        : EndsWithImageExtension(firstEmbed.Image.GetValueOrDefault().Url)
                            ? firstEmbed.Image.GetValueOrDefault().Url
                            : EndsWithImageExtension(firstEmbed.Url)
                                ? firstEmbed.Url
                                : EndsWithImageExtension(firstEmbed.Thumbnail?.Url)
                                    ? firstEmbed.Thumbnail?.Url
                                    : null;
                }),
            .. message.Attachments.Where(a => EndsWithImageExtension(a.Url))
                                  .Select(a => a.Url),
        ];
        return imageUrls;
    }

    public static bool EndsWithImageExtension(string? url)
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
