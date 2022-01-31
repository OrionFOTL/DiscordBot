using Discord.Interactions;
using Discord.WebSocket;

namespace DiscordBot.Commands
{
    public class InteractionTest : InteractionModuleBase<SocketInteractionContext>
    {
        [ComponentInteraction("my-id")]
        public async Task Lol()
        {
            if (Context.Interaction is not SocketMessageComponent interaction)
            {
                return;
            }

            await interaction.RespondAsync($"{interaction.User.Username} clicked the button!");
        }
    }
}
