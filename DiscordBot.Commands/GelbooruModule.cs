using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands
{
    public class GelbooruModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<GelbooruModule> _logger;

        public GelbooruModule(ILogger<GelbooruModule> logger)
        {
            _logger = logger;
        }

        [Command("embed", RunMode = RunMode.Async)]
        public async Task SampleEmbed()
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "Orion",
                    Url = @"https://www.google.com/",
                    IconUrl = @"https://cdn.discordapp.com/emojis/563646143813386240.png?v=1",
                },
                Color = Color.Teal,
                Description = "Some Description",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Field 1",
                        Value = new Emoji("😉")
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Field 2",
                        Value = new Emoji("😙"),
                        IsInline = true,
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Field 3",
                        Value = new Emoji("🤗"),
                    },
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = "Footer",
                    IconUrl = @"https://cdn.discordapp.com/emojis/769703982322417734.png?v=1",
                },
                ImageUrl = @"https://cdn.discordapp.com/attachments/469986835641270284/887460765181820958/FB_IMG_1631511079430.jpg",
                ThumbnailUrl = @"https://cdn.discordapp.com/attachments/469986835641270284/887451570227265597/1631655335350.gif",
                Timestamp = DateTimeOffset.UtcNow,
                Title = "Title!",
                Url = @"https://www.wp.pl/",
            };

            var reply = await ReplyAsync(embed: embed.Build());

            await Task.Delay(TimeSpan.FromSeconds(5));

            //await reply.ModifyAsync(mp => mp.Content = @"https://cdn.discordapp.com/attachments/469986835641270284/887460765181820958/FB_IMG_1631511079430.jpg");
        }
    }
}
