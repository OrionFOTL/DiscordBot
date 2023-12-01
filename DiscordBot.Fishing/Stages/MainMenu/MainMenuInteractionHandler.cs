using Discord.Interactions;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Stages.Base;

namespace DiscordBot.Features.Fishing.Stages.MainMenu;

internal class MainMenuInteractionHandler(
    DatabaseContext databaseContext,
    IStateHandlerFactory stateHandlerFactory) : InteractionHandler(databaseContext, stateHandlerFactory)
{
    [ComponentInteraction("go-fishing")]
    public Task GoFishing()
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        Fire(State.Trigger.GoFishing);

        return Task.CompletedTask;
    }
}
