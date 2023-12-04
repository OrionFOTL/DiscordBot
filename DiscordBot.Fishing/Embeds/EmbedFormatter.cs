using Discord;
using DiscordBot.Features.Fishing.Services.StaticImages;

namespace DiscordBot.Features.Fishing.Embeds;

internal interface IEmbedFormatter
{
    void ClearMessage(MessageProperties messageProperties);

    Task<EmbedFooterBuilder> GetStandardFooter();
}

internal class EmbedFormatter(IStaticImageFetcher staticImageFetcher) : IEmbedFormatter
{
    public async Task<EmbedFooterBuilder> GetStandardFooter()
    {
        return new EmbedFooterBuilder()
        {
            Text = "Fishing Game",
            IconUrl = await staticImageFetcher.GetImageUri(ResourceImage.GameIcon),
        };
    }

    public void ClearMessage(MessageProperties messageProperties)
    {
        messageProperties.Content = null;
        messageProperties.Attachments = null;
        messageProperties.Embed = null;
        messageProperties.Components = null;
    }
}
