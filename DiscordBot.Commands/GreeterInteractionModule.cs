using Discord;
using Discord.Commands;

namespace DiscordBot.Commands;

public class GreeterInteractionModule : ModuleBase<SocketCommandContext>
{

    [Command("spawner")]
    public async Task Spawn()
    {
        var builder = new ComponentBuilder()
            .WithButton("next", "my-id", style: ButtonStyle.Primary, emote: new Emoji("😊"));

        await ReplyAsync("Here's a button!", components: builder.Build());
    }
}
