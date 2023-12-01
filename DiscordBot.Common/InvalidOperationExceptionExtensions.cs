using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DiscordBot.Common;

public static class InvalidOperationExceptionExtensions
{
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? memberName = null)
    {
        if (argument is null)
        {
            throw new InvalidOperationException($"{memberName} was null.");
        }
    }
}
