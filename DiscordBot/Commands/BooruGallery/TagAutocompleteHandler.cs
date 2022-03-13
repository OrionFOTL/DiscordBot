using System.Collections.Concurrent;
using Discord;
using Discord.Interactions;
using DiscordBot.Services.Interface;

namespace DiscordBot.Commands.BooruGallery
{
    public class TagAutocompleteHandler : AutocompleteHandler
    {
        private readonly ITagClient _tagClient;
        private readonly ILogger<TagAutocompleteHandler> _logger;

        private static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _userAutocompleteTokens = new();

        public TagAutocompleteHandler(ITagClient tagClient, ILogger<TagAutocompleteHandler> logger)
        {
            _tagClient = tagClient;
            _logger = logger;
        }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            var tag = autocompleteInteraction.Data.Current.Value as string;

            if (string.IsNullOrWhiteSpace(tag) || tag.Length <= 1)
            {
                return AutocompletionResult.FromSuccess();
            }

            IEnumerable<(string Tag, int Count)> matchingTags = await GetSimilarTags(tag, context.User.Id);

            if (!matchingTags.Any())
            {
                return AutocompletionResult.FromSuccess();
            }

            var autocompleteResults = matchingTags.Select(t => new AutocompleteResult()
            {
                Name = $"{t.Tag} ({t.Count})",
                Value = t.Tag
            });

            return AutocompletionResult.FromSuccess(autocompleteResults);
        }

        private async Task<IEnumerable<(string Tag, int Count)>> GetSimilarTags(string tag, ulong uid)
        {
            if (_userAutocompleteTokens.TryGetValue(uid, out CancellationTokenSource tokenSource))
            {
                tokenSource.Cancel();
            }

            var cancellationTokenSource = new CancellationTokenSource();
            _userAutocompleteTokens[uid] = cancellationTokenSource;

            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                await Task.Delay(100, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError(DateTimeOffset.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " Sending argument: " + tag + " was cancelled by exception");
                return Array.Empty<(string Tag, int Count)>();
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(DateTimeOffset.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " Sending argument: " + tag + " was cancelled by if");
                return Array.Empty<(string Tag, int Count)>();
            }

            _logger.LogWarning("Getting suggestions for tag: {tag}", tag);

            var tags = await _tagClient.GetSimilarTags(tag);

            _logger.LogWarning("Got suggestion: {suggestion} for tag: {tag}", tags.FirstOrDefault().Tag, tag);

            return tags;
        }
    }
}
