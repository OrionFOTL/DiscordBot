using Discord;
using Discord.Interactions;
using DiscordBot.Services.ArtGallery.Tags;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands.BooruGallery;

public class TagAutocompleteHandler(ILogger<TagAutocompleteHandler> logger, ITagClient tagClient) : AutocompleteHandler
{
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
        logger.LogWarning("Getting suggestions for tag: {tag}", tag);

        var tags = await tagClient.GetSimilarTags(tag);

        logger.LogWarning("Got suggestion: {suggestion} for tag: {tag}", tags.FirstOrDefault().CodedName, tag);

        return tags;
    }
}
