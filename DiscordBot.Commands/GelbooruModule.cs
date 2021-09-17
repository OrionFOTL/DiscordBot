﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using Discord.WebSocket;
using DiscordBot.Clients.Interface;
using DiscordBot.Model;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands
{
    public class GelbooruModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<GelbooruModule> _logger;
        private readonly IBooruClient _booruClient;
        private readonly DiscordSocketClient _discordClient;

        public GelbooruModule(ILogger<GelbooruModule> logger, DiscordSocketClient discordClient, IBooruClient booruClient)
        {
            _logger = logger;
            _booruClient = booruClient;
            _discordClient = discordClient;
        }

        protected override void OnModuleBuilding(CommandService commandService, ModuleBuilder builder)
        {
            base.OnModuleBuilding(commandService, builder);

            _discordClient.ButtonExecuted += async (interaction) =>
            {
                _ = Task.Run(() => OnButtonExecuted(interaction));
                await Task.CompletedTask;
            };
        }

        private async Task OnButtonExecuted(SocketMessageComponent interaction)
        {
            string[] customValues = interaction.Data.CustomId.Split(' ');

            if (customValues.First() == "paginator")
            {
                var invokerMessage = await interaction.Channel.GetMessageAsync(interaction.Message.Reference.MessageId.Value);
                if (interaction.User.Id != invokerMessage.Author.Id)
                {
                    return;
                }

                bool forward = customValues.Contains("next");

                Task loaderMessage = interaction.Message.ModifyAsync(mp =>
                {
                    mp.Embed = interaction.Message.Embeds.First()
                        .ToEmbedBuilder()
                        .WithTitle($"Loading {(forward ? "next" : "previous")} one...")
                        .WithUrl(null)
                        .Build();
                    mp.Components = new ComponentBuilder()
                        .WithButton(customId: "previous", style: ButtonStyle.Secondary, emote: new Emoji("◀"), disabled: true)
                        .WithButton(customId: "next", style: ButtonStyle.Secondary, emote: new Emoji("▶"), disabled: true)
                        .Build();
                });

                var channelNsfw = ((SocketTextChannel)interaction.Channel).IsNsfw;
                var tags = invokerMessage.Content.Remove(0, "top ".Length);

                int requestedPage = forward ? Convert.ToInt32(customValues.Last()) + 1 : Convert.ToInt32(customValues.Last()) - 1;

                var updatedImage = await _booruClient.GetImageAsync(amount: 1, page: requestedPage, top: true, noVideo: true, allowNsfw: channelNsfw, contentTags: tags.Split(' '));

                await loaderMessage;
                await interaction.ModifyOriginalResponseAsync(mp =>
                {
                    mp.Embed = new EmbedBuilder
                    {
                        Title = $"Top #{requestedPage} art:",
                        Url = updatedImage.PostUrl,
                        ImageUrl = updatedImage.FileUrl.ToString(),
                    }.Build();
                    mp.Components = new ComponentBuilder()
                        .WithButton(customId: $"paginator previous {requestedPage}", style: ButtonStyle.Secondary, emote: new Emoji("◀"))
                        .WithButton(customId: $"paginator next {requestedPage}", style: ButtonStyle.Secondary, emote: new Emoji("▶"))
                        .Build();
                });
            }
        }

        [Command("top", RunMode = RunMode.Async)]
        public async Task ShowTopImagesFetching(params string[] tags)
        {
            int startingPage = 1;
            var channelNsfw = ((SocketTextChannel)Context.Channel).IsNsfw;

            Task<IUserMessage> fetchingReplyTask = Context.Message.ReplyAsync(embed: new EmbedBuilder().WithDescription("Fetching...").Build(), allowedMentions: AllowedMentions.None);

            Post image = await _booruClient.GetImageAsync(
                amount: 1,
                page: startingPage,
                top: true,
                noVideo: true,
                allowNsfw: channelNsfw,
                tags);

            IUserMessage fetchingReply = await fetchingReplyTask;

            if (image is null)
            {
                await fetchingReply.ModifyAsync(m => m.Embed = new EmbedBuilder().WithDescription("No images found.").Build());
                return;
            }

            var imageEmbed = new EmbedBuilder
            {
                Title = "Top #1 art:",
                Url = image.PostUrl,
                ImageUrl = image.FileUrl.ToString(),
            };

            await fetchingReply.ModifyAsync(m =>
            {
                m.Embed = imageEmbed.Build();
                m.Components = new ComponentBuilder()
                    .WithButton(customId: $"paginator previous {startingPage}", style: ButtonStyle.Secondary, emote: new Emoji("◀"), disabled: true)
                    .WithButton(customId: $"paginator next {startingPage}", style: ButtonStyle.Secondary, emote: new Emoji("▶"))
                    .Build();
            });
        }
    }
}
