using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DiscordBot.Commands;

#if !DEBUG
[DontAutoRegister]
#endif

public class InteractionTestsModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("spawner", "Posts a message with a button")]
    public async Task Spawn(string ignored)
    {
        var builder = new ComponentBuilder()
            .WithButton("next", "my-id", style: ButtonStyle.Primary, emote: new Emoji("😊"));

        await RespondAsync("Here's a button!", components: builder.Build());
    }

    [SlashCommand("spawnmenu", "Spawns select menu")]
    public async Task SpawnMenu()
    {
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select an option")
            .WithCustomId("menu-1")
            .WithMinValues(1)
            .WithMaxValues(1)
            .AddOption("Option A", "opt-a", "Option B is lying!")
            .AddOption("Option B", "opt-b", "Option A is telling the truth!", new Emoji("🍌"));

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);

        await RespondAsync("Whos really lying?", components: builder.Build());
    }

    [ComponentInteraction("menu-*")]
    public async Task HandleSelected(string _, string[] values)
    {
        await RespondAsync("Chosen options: " + string.Join(',', values));
    }

    [SlashCommand("kek2", "posts a cat")]
    public async Task Kitty(string name)
    {
        await DeferAsync(true);

        await Task.Delay(5_000);

        await ModifyOriginalResponseAsync(mp => mp.Embed = new EmbedBuilder().WithDescription("hi").Build());

        await Task.Delay(2_000);

        await ModifyOriginalResponseAsync(mp => mp.Content = "Test!");
    }

    [ComponentInteraction("my-id", runMode: RunMode.Async)]
    public async Task ButtonResponse()
    {
        /* After you DeferAsync or RespondAsync or UpdateAsync (only for Buttons),
         * you can only ModifyOriginalResponse later
         * FollowupAsync is for updating DeferAsync! */

        var interaction = (SocketMessageComponent)Context.Interaction;

        //await interaction.DeferAsync();

        await Task.Delay(2000);
        await interaction.UpdateAsync(mp => mp.Content = DateTime.Now.ToString());



        await Task.Delay(2000);
        await ModifyOriginalResponseAsync(mp => mp.Content = DateTime.Now.ToString());
        //await ModifyOriginalResponseAsync(mp => mp.Content = DateTime.Now.ToString());
    }

    [SlashCommand("test", "test")]
    public async Task Test()
    {
        //await DeferAsync();
        //await ReplyAsync("Reply - just posts a message");

        await Task.Delay(2000);
        await RespondAsync("Respond - Orion uses /test");

        await Task.Delay(3000);
        await ModifyOriginalResponseAsync(mp => mp.Content = "Modifies message with Orion uses /test");
    }

    [SlashCommand("confirm", "Prints a confirmation dialog")]
    public async Task Confirm()
    {
        await DeferAsync();

        var response = await InteractionUtility.ConfirmAsync(Context.Client, Context.Channel, TimeSpan.FromSeconds(30));

        await FollowupAsync($"You responded with {response}");
    }
}
