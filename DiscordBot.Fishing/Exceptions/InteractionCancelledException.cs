namespace DiscordBot.Features.Fishing.Exceptions;

internal class InteractionCancelledException : Exception
{
    public InteractionCancelledException()
    {
    }

    public InteractionCancelledException(string? message) : base(message)
    {
    }
}
