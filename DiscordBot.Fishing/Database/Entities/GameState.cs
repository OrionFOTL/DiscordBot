using System.ComponentModel.DataAnnotations.Schema;
using DiscordBot.Features.Fishing.Database.Entities.Equipment;
using DiscordBot.Features.Fishing.State;
using DiscordBot.Fishing.State;
using Stateless;

namespace DiscordBot.Features.Fishing.Database.Entities;

internal class GameState
{
    [NotMapped]
    public StateMachine<StateEnum, Trigger> StateMachine { get; }

    public int Id { get; init; }

    public StateEnum State { get; private set; }

    public string? Message { get; set; }

    public int PlayerId { get; set; }

    public required Player Player { get; init; }

    public Location? Location { get; set; }

    public OwnedItem? ViewingOwnedItem { get; set; }

    public GameState(StateEnum state)
    {
        State = state;
        StateMachine = InitialiseStateMachine();
    }

    private StateMachine<StateEnum, Trigger> InitialiseStateMachine()
    {
        var stateMachine = new StateMachine<StateEnum, Trigger>(() => State, state => State = state);

        stateMachine.Configure(StateEnum.MainMenu)
            .Permit(Trigger.GoToLocationSelect, StateEnum.LocationSelection)
            .Permit(Trigger.ViewEquipment, StateEnum.EquipmentView);

        stateMachine.Configure(StateEnum.LocationSelection)
            .PermitIf(Trigger.LocationSelected, StateEnum.OnLocation, () => Location is not null, $"{nameof(Location)} not null");

        stateMachine.Configure(StateEnum.OnLocation)
            .Permit(Trigger.GoToLocationSelect, StateEnum.LocationSelection)
            .Permit(Trigger.GoToMenu, StateEnum.MainMenu)
            .Permit(Trigger.ViewEquipment, StateEnum.EquipmentView)
            .Permit(Trigger.ViewFishAtLocation, StateEnum.FishListAtLocation)
            .Permit(Trigger.ThrowLine, StateEnum.FishingInProgress);

        stateMachine.Configure(StateEnum.EquipmentView)
            .Permit(Trigger.EquipmentItemSelected, StateEnum.EquipmentItemView)
            .Permit(Trigger.GoToMenu, StateEnum.MainMenu);

        stateMachine.Configure(StateEnum.EquipmentItemView)
            .Permit(Trigger.ViewEquipment, StateEnum.EquipmentView)
            .Permit(Trigger.EquipItem, StateEnum.EquipmentView);

        stateMachine.Configure(StateEnum.FishListAtLocation)
            .Permit(Trigger.GoBack, StateEnum.OnLocation);

        stateMachine.Configure(StateEnum.FishingInProgress)
            .Permit(Trigger.FishingCompleted, StateEnum.FishingResults);

        stateMachine.Configure(StateEnum.FishingResults)
            .Permit(Trigger.GoBack, StateEnum.MainMenu);

        return stateMachine;
    }
}
