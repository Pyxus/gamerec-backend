
namespace GameRec.Api.Util;

/// <summary>
/// Simple utility class for loading environment variables from a ".env" file.
/// </summary>
public static class DotEnv
{

    public static void Load()
    {
        var root = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(root, ".env");
        Console.WriteLine(filePath);
        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split(
                '=',
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
}
