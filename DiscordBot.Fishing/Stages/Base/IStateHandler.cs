﻿using Discord;
using DiscordBot.Features.Fishing.Database.Entities;

namespace DiscordBot.Features.Fishing.Stages.Base;

internal interface IStateHandler
{
    Task Handle(IDiscordInteraction interaction, GameState gameState);
}