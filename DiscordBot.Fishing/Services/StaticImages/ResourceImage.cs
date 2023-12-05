namespace DiscordBot.Features.Fishing.Services.StaticImages;

internal class ResourceImage
{
    private static readonly IEnumerator<ResourceImage> _welcomeScreenEnumerator = WelcomeScreensRotation().GetEnumerator();

    private ResourceImage(string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; }

    public static ResourceImage GameIcon { get; } = new(nameof(FishingResources.GameIcon));

    public static ResourceImage WelcomeScreen { get { _welcomeScreenEnumerator.MoveNext(); return _welcomeScreenEnumerator.Current; } }

    public static ResourceImage EquipmentScreen { get; } = new(nameof(FishingResources.Equipment1));

    private static IEnumerable<ResourceImage> WelcomeScreensRotation()
    {
        ResourceImage resourceImage2 = new(nameof(FishingResources.TitleScreen2));
        ResourceImage resourceImage3 = new(nameof(FishingResources.TitleScreen3));
        ResourceImage resourceImage4 = new(nameof(FishingResources.TitleScreen4));
        ResourceImage resourceImage5 = new(nameof(FishingResources.TitleScreen5));

        while (true)
        {
            yield return resourceImage4;
            yield return resourceImage2;
            yield return resourceImage3;
            yield return resourceImage5;
        }
    }
}
