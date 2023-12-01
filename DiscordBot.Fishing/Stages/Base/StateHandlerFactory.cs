using DiscordBot.Features.Fishing.Stages.LocationSelection;
using DiscordBot.Features.Fishing.Stages.MainMenu;
using DiscordBot.Features.Fishing.Stages.OnLocation;
using DiscordBot.Fishing.State;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Features.Fishing.Stages.Base;

internal interface IStateHandlerFactory
{
    IStateHandler GetStateHandler(StateEnum state);
}

internal class StateHandlerFactory(IServiceProvider serviceProvider) : IStateHandlerFactory
{
    public IStateHandler GetStateHandler(StateEnum state)
    {
        return state switch
        {
            StateEnum.MainMenu => serviceProvider.GetRequiredService<IMainMenuStateHandler>(),
            StateEnum.LocationSelection => serviceProvider.GetRequiredService<ILocationSelectionStateHandler>(),
            StateEnum.OnLocation => serviceProvider.GetRequiredService<IOnLocationStateHandler>(),
        };
    }
}