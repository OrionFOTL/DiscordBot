using Discord;
using Discord.Interactions;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.State;
using DiscordBot.Fishing.State;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Stages.Base;

internal abstract class InteractionHandler(
    DatabaseContext databaseContext,
    IStateHandlerFactory stateHandlerFactory) : InteractionModuleBase
{
    protected DatabaseContext DatabaseContext { get; } = databaseContext;

    protected GameState? GameState { get; set; }

    public override async Task BeforeExecuteAsync(ICommandInfo commandInfo)
    {
        await Context.Interaction.DeferAsync();

        IUser invokingUser = Context.Interaction.User;

        if (Context.Interaction is IComponentInteraction componentInteraction)
        {
            IUser originalInvoker = componentInteraction.Message.Interaction.User;

            if (invokingUser.Id != originalInvoker.Id)
            {
                throw new InvalidOperationException("You can't press other ppl's buttons");
            }

            invokingUser = originalInvoker;
        }

        var gameState = await DatabaseContext.GameStates.FirstOrDefaultAsync(gs => gs.Player.DiscordId == invokingUser.Id)
                     ?? await RegisterPlayer(invokingUser);

        GameState = gameState;
    }

    public override async Task AfterExecuteAsync(ICommandInfo commandInfo)
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        var stateHandler = stateHandlerFactory.GetStateHandler(GameState.StateMachine.State);
        await stateHandler.Handle(Context.Interaction, GameState);

        await DatabaseContext.SaveChangesAsync();
    }

    protected void Fire(Trigger trigger)
    {
        InvalidOperationExceptionExtensions.ThrowIfNull(GameState);

        if (!GameState.StateMachine.CanFire(trigger, out var unmetGuards))
        {
            // exit somehow
            throw new InvalidOperationException($"Unable to fire {trigger}");
        }

        GameState.StateMachine.Fire(trigger);
    }

    protected async Task<GameState> RegisterPlayer(IUser invokingUser)
    {
        var player = DatabaseContext.Players.FirstOrDefault(p => p.DiscordId == invokingUser.Id)
            ?? new Player
            {
                DiscordId = invokingUser.Id,
                DiscordName = invokingUser.GlobalName,
            };

        var newGameState = new GameState(StateEnum.MainMenu)
        {
            Player = player
        };

        DatabaseContext.GameStates.Add(newGameState);

        await DatabaseContext.SaveChangesAsync();

        return newGameState;
    }
}
