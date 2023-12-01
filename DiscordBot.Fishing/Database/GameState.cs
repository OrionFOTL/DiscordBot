using System.ComponentModel.DataAnnotations.Schema;
using DiscordBot.Features.Fishing.State;
using DiscordBot.Fishing.State;
using Stateless;

namespace DiscordBot.Features.Fishing.Database;

internal class GameState
{
    [NotMapped]
    public StateMachine<StateEnum, Trigger> StateMachine { get; }

    public StateEnum State { get; private set; }

    public int Id { get; init; }

    public int PlayerId { get; set; }

    public required Player Player { get; init; }

    public string? Message { get; set; }

    public string? CurrentLocationCode { get; set; }

    public GameState(StateEnum state)
    {
        State = state;
        StateMachine = InitialiseStateMachine();
    }

    private StateMachine<StateEnum, Trigger> InitialiseStateMachine()
    {
        var stateMachine = new StateMachine<StateEnum, Trigger>(() => State, state => State = state);

        stateMachine.Configure(StateEnum.MainMenu)
            .Permit(Trigger.GoFishing, StateEnum.LocationSelection);

        stateMachine.Configure(StateEnum.LocationSelection)
            .PermitIf(Trigger.LocationSelected, StateEnum.OnLocation, () => CurrentLocationCode is not null, $"{nameof(CurrentLocationCode)} not null");

        stateMachine.Configure(StateEnum.OnLocation)
            .Permit(Trigger.BackToLocationSelect, StateEnum.LocationSelection)
            .Permit(Trigger.BackToMenu, StateEnum.MainMenu)
            .Permit(Trigger.ViewEquipment, StateEnum.EquipmentView)
            .Permit(Trigger.ViewFishAtLocation, StateEnum.FishListAtLocation)
            .Permit(Trigger.ThrowLine, StateEnum.FishingInProgress);

        stateMachine.Configure(StateEnum.EquipmentView)
            .Permit(Trigger.EquipmentTypeSelected, StateEnum.SpecificEquipmentSelection)
            .Permit(Trigger.EquipmentConfirmed, StateEnum.OnLocation);

        stateMachine.Configure(StateEnum.SpecificEquipmentSelection)
            .Permit(Trigger.SpecificEquipmentSelected, StateEnum.EquipmentView);

        stateMachine.Configure(StateEnum.FishListAtLocation)
            .Permit(Trigger.GoBack, StateEnum.OnLocation);

        stateMachine.Configure(StateEnum.FishingInProgress)
            .Permit(Trigger.FishingCompleted, StateEnum.FishingResults);

        stateMachine.Configure(StateEnum.FishingResults)
            .Permit(Trigger.GoBack, StateEnum.MainMenu);

        return stateMachine;
    }
}
