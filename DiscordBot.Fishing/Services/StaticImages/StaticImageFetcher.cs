using Discord;
using Discord.WebSocket;
using DiscordBot.Features.Fishing.Database;
using DiscordBot.Features.Fishing.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordBot.Features.Fishing.Services.StaticImages;

internal interface IStaticImageFetcher
{
    public Task<string> GetImageUri(ResourceImage resourceImage);
}


internal class StaticImageFetcher(
    ILogger<StaticImageFetcher> logger,
    IOptions<ImageSaverSettings> options,
    DatabaseContext databaseContext,
    DiscordSocketClient discordClient) : IStaticImageFetcher
{
    public async Task<string> GetImageUri(ResourceImage resourceImage)
    {
        SavedImage? savedImage = await databaseContext.SavedImages.FirstOrDefaultAsync(i => i.Name == resourceImage.Identifier);

        if (savedImage is not null)
        {
            return savedImage.Uri.ToString();
        }

        logger.LogInformation("Url for resource image {imageIdentifier} not found in db; uploading", resourceImage.Identifier);

        byte[]? file = (byte[]?)FishingResources.ResourceManager.GetObject(resourceImage.Identifier)
            ?? throw new InvalidOperationException($"Resource with name {resourceImage.Identifier} not found in resource file");

        using var fileStream = new MemoryStream(file);

        var channel = await discordClient.GetChannelAsync(options.Value.ImageUploadingChannelId);

        if (channel is not ITextChannel textChannel)
        {
            throw new InvalidOperationException($"Channel {channel.Id}:{channel.Name} is not a text channel");
        }

        string filename = resourceImage.Identifier + ".png";

        var message = await textChannel.SendFileAsync(fileStream, filename)
            ?? throw new InvalidOperationException($"Uploading an image responded in a null message");

        if (message.Attachments.Count is 0)
        {
            throw new InvalidOperationException("Uploaded image message has no attachments");
        }

        string uploadedImageUrl = message.Attachments.First().Url;

        logger.LogInformation("Image {imageName} uploaded to Discord with name {filename} to url {fileUrl}", resourceImage.Identifier, filename, uploadedImageUrl);

        databaseContext.SavedImages.Add(new SavedImage { Name = resourceImage.Identifier, Uri = new Uri(uploadedImageUrl) });
        await databaseContext.SaveChangesAsync();

        return uploadedImageUrl;
    }
}
