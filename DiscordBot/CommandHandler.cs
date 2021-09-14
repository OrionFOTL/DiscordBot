using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Commands;

namespace DiscordBot
{
    public class CommandHandler
    {
        private const char _commandPrefix = '$';

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider)
        {
            _client = client;
            _commands = commands;
            _serviceProvider = serviceProvider;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(typeof(EggplantModule).Assembly, _serviceProvider);
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            if (message is not SocketUserMessage userMessage || userMessage.Source != MessageSource.User)
            {
                return;
            }

            var commandStartPos = 0;
            if (!userMessage.HasCharPrefix(_commandPrefix, ref commandStartPos))
            {
                //return;
            }

            var commandContext = new SocketCommandContext(_client, userMessage);

            await _commands.ExecuteAsync(commandContext, commandStartPos, _serviceProvider);
        }
    }
}
