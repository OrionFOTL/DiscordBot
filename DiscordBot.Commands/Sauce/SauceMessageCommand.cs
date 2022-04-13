using Discord;
using Discord.Commands;
using DiscordBot.Services.Interface;

namespace DiscordBot.Commands.Sauce;

public class SauceMessageCommand : ModuleBase<SocketCommandContext>
{
    private readonly ISauceClient _sauceClient;

    public SauceMessageCommand(ISauceClient sauceClient)
    {
        _sauceClient = sauceClient;
    }

    [Command("sauce")]
    public async Task GetSauce()
    {
        if (Context.Message.ReferencedMessage is null)
        {
            await Context.Message.ReplyAsync("Right-click some message and choose Applications > Get Sauce, or say \"sauce\" with a quoted message.");
            return;
        }

        var imageUrls = SourceContextCommand.ExtractImageUrlsFromMessage(Context.Message.ReferencedMessage);

        var urlsWithSauce = await _sauceClient.GetSauce(imageUrls);

        var embeds = urlsWithSauce.Select(s => SourceContextCommand.MakeEmbedFromSauces(s.Sauces)
            .WithImageUrl(s.Url)
            .WithFooter(new EmbedFooterBuilder { Text = $"Invoked by {Context.Message.Author}" })
            .Build());

        await Context.Message.ReferencedMessage.ReplyAsync(
            allowedMentions: new AllowedMentions { MentionRepliedUser = false },
            embeds: embeds.ToArray());
    }
}
