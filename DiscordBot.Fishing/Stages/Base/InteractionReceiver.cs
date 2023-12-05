using Discord;
using Discord.Interactions;
using DiscordBot.Common;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Entities.Equipment;
using DiscordBot.Features.Fishing.Exceptions;
using DiscordBot.Features.Fishing.State;
using DiscordBot.Fishing.State;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Features.Fishing.Stages.Base;

internal abstract class InteractionReceiver(
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
                await Context.Interaction.FollowupAsync(
                    $"{MentionUtils.MentionUser(invokingUser.Id)} You can't press other people's buttons. To play your own game, type `/fishing-game`", ephemeral: true);
                throw new InteractionCancelledException("Invoking user is not original player");
            }

            invokingUser = originalInvoker;
        }

        var gameState = await DatabaseContext.GameStates.Include(s => s.Player).FirstOrDefaultAsync(gs => gs.Player.DiscordId == invokingUser.Id)
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

        if (!GameState.StateMachine.GetPermittedTriggers().Contains(trigger))
        {
            throw new InvalidOperationException($"Trigger {trigger} is not permitted in state {GameState.State}");
        }

        if (!GameState.StateMachine.CanFire(trigger, out var unmetGuards))
        {
            throw new InvalidOperationException($"Unable to fire {trigger}; unmet guards: [{string.Join(',', unmetGuards ?? [])}]");
        }

        GameState.StateMachine.Fire(trigger);
    }

    protected async Task<GameState> RegisterPlayer(IUser invokingUser)
    {
        var basicRod = DatabaseContext.Items.OfType<FishingRod>().First();
        var basicBait = DatabaseContext.Items.OfType<Bait>().First();

        var player = DatabaseContext.Players.FirstOrDefault(p => p.DiscordId == invokingUser.Id)
            ?? new Player
            {
                DiscordId = invokingUser.Id,
                DiscordName = invokingUser.GlobalName,
                OwnedItems = [],
            };

        var newGameState = new GameState(StateEnum.MainMenu)
        {
            Player = player
        };

        player.OwnedItems = [
            new OwnedFishingRod { Player = player, Item = basicRod, Equipped = true },
            new OwnedBait { Player = player, Item = basicBait, Amount = 10, Equipped = true },
        ];

        DatabaseContext.GameStates.Add(newGameState);

        await DatabaseContext.SaveChangesAsync();

        return newGameState;
    }
}
