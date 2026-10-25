namespace RoboRoute.Winforms.Core;

// The built-in levels, in play order. They are no longer written in code: they are read from
// Levels/levels.txt (next to the .exe) by LevelParser. To add a level, add a [Level] block to that file.
public static class Levels
{
    private static List<LevelData> _all = new List<LevelData>();

    // Empty until LoadBuiltIn() has run (and stays empty if the file couldn't be read)
    public static IReadOnlyList<LevelData> All => _all;

    public static string FilePath => Path.Combine(AppContext.BaseDirectory,"Levels","levels.txt");

    // Loads the built-in file. Returns a message for each level that was skipped because of a problem.
    // File problems (missing file, no permission) and structure problems throw: the caller decides what to show.
    public static List<string> LoadBuiltIn()
    {
        LoadResult result = LevelParser.LoadAll(FilePath);
        _all = result.Levels;
        return result.Skipped;
    }
    public static string FindLevelFile()
    {
        string[] paths =
        {
        Path.Combine(
            AppContext.BaseDirectory,
            "Levels", "levels.txt"),

        Path.Combine(
            Environment.CurrentDirectory,
            "Levels", "levels.txt"),

        Path.Combine(
            AppContext.BaseDirectory,
            "levels.txt")
    };

        foreach (string path in paths)
        {
            if (File.Exists(path))
                return path;
        }

        throw new FileNotFoundException(
            "Could not find levels.txt. Checked:\n" +
            string.Join("\n", paths));
    }
}

