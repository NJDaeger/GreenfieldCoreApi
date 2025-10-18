namespace GreenfieldCoreDataAccess.Database.ScriptManager;

public record Script(string FilePath, List<string> DependsOn, bool IsInit, bool IsSproc, string AppliesTo, int Major, int Minor)
{

    public static Script FromFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be determined from the provided path.", nameof(filePath));

        var parts = Path.GetFileNameWithoutExtension(filePath).Split('_');
        if (parts.Length < 3)
            throw new FormatException($"Invalid script filename format: {fileName}");

        var isInit = parts[0].Equals("init", StringComparison.OrdinalIgnoreCase);
        var isSproc = parts[1].Equals("usp", StringComparison.OrdinalIgnoreCase);
        var appliesTo = parts[1] + (isSproc ? "_" + parts[2] : "");

        var (major, minor) = ParseVersion(parts[^1]);

        return new Script(filePath, ResolveDependencies(filePath), isInit, isSproc, appliesTo, major, minor);
    }
    
    private static List<string> ResolveDependencies(string filePath)
    {
        var dependencies = new List<string>();
        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            if (!line.StartsWith("-- DependsOn:", StringComparison.OrdinalIgnoreCase)) continue;
            var deps = line[(line.IndexOf(':') + 1)..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            dependencies.AddRange(deps);
        }

        return dependencies;
    }
    
    private static (int Major, int Minor) ParseVersion(string versionPart)
    {
        var versionSegments = versionPart.Replace("v", "").Split('.');
        if (versionSegments.Length != 2 ||
            !int.TryParse(versionSegments[0], out var major) ||
            !int.TryParse(versionSegments[1], out var minor))
        {
            throw new FormatException($"Invalid version format: {versionPart}");
        }

        return (major, minor);
    }
    
}