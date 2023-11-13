namespace DiscordBot.Extensions;

internal static class ServiceCollectionOptionsExtensions
{
    public static IServiceCollection AddAndValidateOptions<TOptions>(this IServiceCollection services, string configurationSection = null)
        where TOptions : class
    {
        return services
            .AddOptions<TOptions>()
            .BindConfiguration(configurationSection ?? typeof(TOptions).Name)
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Services;
    }
}
