using Discord.Interactions;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;

namespace DiscordBot.Features.Fishing.Stages.StartGame;

internal class StartGameCommandHandler(
    DatabaseContext databaseContext,
    IStateHandlerFactory stateHandlerFactory) : InteractionHandler(databaseContext, stateHandlerFactory)
{
    [SlashCommand("fishing_game", "Play a fishing game!")]
    public Task ShowGame()
    {
        return Task.CompletedTask;
    }
}
