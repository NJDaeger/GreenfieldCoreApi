namespace GreenfieldCoreServices.Extensions;

public static class HelperExtensions
{
    public static bool IsAnyOf(this string? source, params string[] values)
    {
        return source is not null && values.Any(value => source.Equals(value, StringComparison.OrdinalIgnoreCase));
    }
}