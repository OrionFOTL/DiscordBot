using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BooruDex.Booru;
using BooruDex.Booru.Client;
using BooruDex.Exceptions;
using DiscordBot.Clients.Interface;
using DiscordBot.Model;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Clients
{
    public class BooruClient : IBooruClient
    {
        private readonly ILogger<BooruClient> _logger;
        private readonly Booru _booru;

        public BooruClient(ILogger<BooruClient> logger)
        {
            _logger = logger;
            _booru = new Gelbooru();

        }
        public async Task<IEnumerable<Post>> GetImagesAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
        {
            var tags = contentTags.ToList();

            tags.Add(Encoding.UTF8.GetString(Convert.FromBase64String("LWxvbGk=")));

            if (top)
            {
                tags.Add("sort:score");
            }

            if (noVideo)
            {
                tags.Add("-video -mp4 -webm");
            }

            if (!allowNsfw)
            {
                tags.Add("rating:safe");
            }

            try
            {
                var posts = await _booru.PostListAsync((byte)amount, tags.ToArray(), (uint)page);

                return posts.Select(p => new Post
                {
                    PostUrl = p.PostUrl,
                    FileUrl = p.FileUrl,
                    PreviewUrl = p.PreviewUrl,
                    Tags = p.Tags,
                    Source = p.Source
                });
            }
            catch (SearchNotFoundException)
            {
                return Array.Empty<Post>();
            }
        }
        public async Task<Post> GetImageAsync(int amount, int page, bool top = true, bool noVideo = true, bool allowNsfw = false, params string[] contentTags)
        {
            IEnumerable<Post> posts = await GetImagesAsync(amount, page, top, noVideo, allowNsfw, contentTags);

            return posts.FirstOrDefault();
        }
    }
}
