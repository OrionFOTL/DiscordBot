using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using Discord.WebSocket;

namespace DiscordBot.Commands;

public class GreeterInteractionModule : ModuleBase<SocketCommandContext>
{
    private readonly DiscordSocketClient _client;

    public GreeterInteractionModule(DiscordSocketClient client)
    {
        _client = client;
    }

    protected override void OnModuleBuilding(CommandService commandService, ModuleBuilder builder)
    {
        base.OnModuleBuilding(commandService, builder);

        _client.ButtonExecuted += Client_ButtonExecuted;
    }

    private async Task Client_ButtonExecuted(SocketMessageComponent interaction)
    {
        if (interaction.Data.CustomId == "my-id")
        {
            await interaction.ModifyOriginalResponseAsync(mp => mp.Content = $"{interaction.User.Username} clicked this!");
        }
    }

    [Command("spawner")]
    public async Task Spawn()
    {
        var builder = new ComponentBuilder()
            .WithButton("next", "my-id", style: ButtonStyle.Primary, emote: new Emoji("😊"));

        await ReplyAsync("Here's a button!", component: builder.Build());
    }
}
