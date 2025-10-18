namespace GreenfieldCoreServices.Commands;

public static class CommandHelpers
{
    public static T? GetArg<T>(this string[] args, int index, T? defVal = default)
    {
        if (args.Length <= index)
            return defVal;

        return (T?)Convert.ChangeType(args[index], typeof(T));
    }
}