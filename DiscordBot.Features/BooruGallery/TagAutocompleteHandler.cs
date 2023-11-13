using Discord;
using Discord.Interactions;
using DiscordBot.Services.ArtGallery.Tags;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands.BooruGallery;

public class TagAutocompleteHandler : AutocompleteHandler
{
    private readonly ITagClient _tagClient;
    private readonly ILogger<TagAutocompleteHandler> _logger;

    public TagAutocompleteHandler(ITagClient tagClient, ILogger<TagAutocompleteHandler> logger)
    {
        _tagClient = tagClient;
        _logger = logger;
    }

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
    {
        if (autocompleteInteraction.Data.Current.Value is not (string and { Length: > 1 } tag)
            || string.IsNullOrWhiteSpace(tag))
        {
            return AutocompletionResult.FromSuccess();
        }

        tag = tag.Replace(' ', '_');
        IEnumerable<Tag> matchingTags = await GetSimilarTags(tag);

        if (!matchingTags.Any())
        {
            return AutocompletionResult.FromSuccess();
        }

        var autocompleteResults = matchingTags.Select(t => new AutocompleteResult()
        {
            Name = $"{t.PrettyName} ({t.Count})",
            Value = t.CodedName
        });

        return AutocompletionResult.FromSuccess(autocompleteResults);
    }

    private async Task<IEnumerable<Tag>> GetSimilarTags(string tag)
    {
        _logger.LogWarning("Getting suggestions for tag: {tag}", tag);

        var tags = await _tagClient.GetSimilarTags(tag);

        _logger.LogWarning("Got suggestion: {suggestion} for tag: {tag}", tags.FirstOrDefault().CodedName, tag);

        return tags;
    }
}
