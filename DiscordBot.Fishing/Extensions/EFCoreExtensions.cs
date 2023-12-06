using System.Linq.Expressions;

namespace DiscordBot.Features.Fishing.Extensions;

internal static class EFCoreExtensions
{
    public static IQueryable<T> OfTypes<T>(this IQueryable<T> source, params Type[] types)
    {
        return source.Where(types.MakeFilter<T>());
    }

    private static Expression<Func<T, bool>> MakeFilter<T>(this IEnumerable<Type> types)
    {
        var parameter = Expression.Parameter(typeof(T), "e");

        var body = types.Select(type => Expression.TypeIs(parameter, type))
                        .Aggregate<Expression>(Expression.OrElse);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
