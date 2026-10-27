using System.Globalization;

namespace RoboRoute.Winforms.Core;

// The player's progress, kept in a small text file so it survives closing the game.
//
// What is saved: for each level the player has beaten, the FEWEST blocks they used to beat it.
// That one number does both jobs:
//   - a level has been beaten if it has a score
//   - level N+1 is unlocked once level N has been beaten
//
// File location: %AppData%\RoboRoute\progress.txt 
// File format, one line per beaten level:
//
//   First Steps=7
//   Staircase=5
//
// Levels are matched by NAME, so renaming a level in levels.txt makes its saved score unused.
//
// Nothing in here is allowed to crash the game: a missing, unreadable or damaged save file
// just means "start fresh", and the player is told when something was ignored.
public class SaveData
{
    private readonly Dictionary<string, int> _best = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    public static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RoboRoute", "progress.txt");

    // Set by Load() when part of the save file had to be ignored (null when everything was fine)
    public string? LoadWarning { get; private set; }

    // Reads the save file. Never throws: returns an empty SaveData if the file is missing or can't be read.
    public static SaveData Load()
    {
        var data = new SaveData();

        try
        {
            // 'using' closes the file even if reading fails halfway
            using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);

            int unreadable = 0;
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("//")) continue;

                // Split on the LAST '=' so a level name containing '=' still works
                int equals = line.LastIndexOf('=');
                int blocks = 0;
                bool ok = equals > 0 &&
                          int.TryParse(line.Substring(equals + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out blocks) &&
                          blocks >= 1;

                if (!ok)
                {
                    unreadable++;       // a damaged or hand-edited line: skip it, keep the rest
                    continue;
                }

                data._best[line.Substring(0, equals).Trim()] = blocks;
            }

            if (unreadable > 0)
                data.LoadWarning = unreadable + " unreadable line(s) in your save file were ignored.";
        }
        catch (FileNotFoundException)
        {
            // Normal on the very first run: nothing saved yet
        }
        catch (DirectoryNotFoundException)
        {
            // Same: the RoboRoute folder doesn't exist yet
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            data._best.Clear();
            data.LoadWarning = "Your saved progress couldn't be read, so you are starting fresh:\n" + ex.Message;
        }

        return data;
    }

    // Has the player beaten this level?
    public bool IsCompleted(string levelName) => _best.ContainsKey(levelName);

    // The fewest blocks used to beat the level, or null if it hasn't been beaten
    public int? GetBest(string levelName) => _best.TryGetValue(levelName, out int blocks) ? blocks : null;

    // Notes that the level was beaten using this many blocks.
    // Returns true if that is a new best (including the first time the level is beaten); the caller should then Save().
    public bool Record(string levelName, int blocksUsed)
    {
        if (_best.TryGetValue(levelName, out int previous) && previous <= blocksUsed)
            return false;

        _best[levelName] = blocksUsed;
        return true;
    }

    // Writes the save file. Returns null on success, or a message for the player if it failed.
    public string? Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            using var writer = new StreamWriter(FilePath, append: false);
            writer.WriteLine("// RoboRoute progress. One line per beaten level: <level name>=<fewest blocks used>");

            foreach (KeyValuePair<string, int> entry in _best)
                writer.WriteLine(entry.Key + "=" + entry.Value);

            return null;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            return "Your progress couldn't be saved:\n\n" + ex.Message;
        }
    }
}
