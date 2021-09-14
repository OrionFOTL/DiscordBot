using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DiscordBot.Commands
{
    public class EggplantModule : ModuleBase<SocketCommandContext>
    {
        [Command("dog")]
        public async Task PrintEggplant()
        {
            await Context.Channel.SendMessageAsync("🌭");
        }

        [Command("orion")]
        public async Task ReactToOrion()
        {
            await Context.Message.AddReactionAsync(new Emoji("🌌"));
            await Context.Message.AddReactionAsync(new Emoji("💫"));
        }

        [Command("thicc")]
        public async Task ThiccenThis([Remainder] string textToThiccen)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder().WithName("me"),
                Title = "Interesting",
            }.Build();

            await Context.Message.ReplyAsync($"**{textToThiccen}**", embed: embed);
        }

        [Command("user")]
        public async Task ShowUser(IUser user)
        {
            await ReplyAsync(user.CreatedAt.ToString());
        }
    }
}
