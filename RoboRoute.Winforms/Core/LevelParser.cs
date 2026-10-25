using System.Globalization;
using static RoboRoute.Winforms.Core.Robot;

namespace RoboRoute.Winforms.Core;

// What LoadAll gives back: the levels that loaded (in play order) and a message for each level that was skipped
public sealed record LoadResult(List<LevelData> Levels, List<string> Skipped);

// Reads levels from text files. This is the only class that knows the file format, so the rest of the
// program just asks for LevelData and never sees a file.
//
// File format (one file can hold many levels; // starts a comment line):
//
//   [Level]
//   Order=1                 <- play order, required in the built-in file only
//   Name=First Steps
//   MaxBlocks=8
//   MoveForward=7           <- how many of each block the player is given (0 = not available)
//   TurnLeft=0
//   TurnRight=2
//   Repeat=0
//   Map:
//   WWWWWWW                 <- W wall, F floor, S start, G goal, H hazard
//   WSFFFGW
//   WWWWWWW
//   [End]
//
// Problems are reported with LevelFormatException. File problems (missing file, no permission...)
// come out as the normal IOException family and are left for the caller to handle.
public static class LevelParser
{
    private static readonly string[] RequiredKeys = { "Name", "Facing", "MaxBlocks", "MoveForward", "TurnLeft", "TurnRight", "Repeat" };
    private static readonly string[] KnownKeys = { "Order", "Name", "Facing", "MaxBlocks", "MoveForward", "TurnLeft", "TurnRight", "Repeat" };

    // The lines of one [Level] ... [End] block, each with its line number in the file
    private sealed class RawLevel
    {
        public int StartLine;
        public List<(int Line, string Text)> Lines = new List<(int, string)>();
    }

    // For the Import button: reads only the first level in the file and ignores the rest.
    public static LevelData LoadFirstLevel(string path)
    {
        string file = Path.GetFileName(path);

        List<RawLevel> raw = ReadRawLevels(path, file, firstOnly: true);
        if (raw.Count == 0)
            throw new LevelFormatException(file, 0, "no level found. A level starts with [Level] and ends with [End].");

        return ParseLevel(raw[0], file, requireOrder: false, out _);
    }

    // For the built-in levels file: reads every level and sorts them by their Order line.
    // A level with a problem is skipped and reported in Skipped, so one typo doesn't remove every level.
    public static LoadResult LoadAll(string path)
    {
        string file = Path.GetFileName(path);

        var parsed = new List<(int Order, LevelData Level)>();
        var skipped = new List<string>();

        foreach (RawLevel raw in ReadRawLevels(path, file, firstOnly: false))
        {
            try
            {
                LevelData level = ParseLevel(raw, file, requireOrder: true, out int order);
                parsed.Add((order, level));
            }
            catch (LevelFormatException ex)
            {
                skipped.Add(ex.Message);
            }
        }

        // OrderBy keeps the file order for levels with the same Order number
        List<LevelData> levels = parsed.OrderBy(p => p.Order).Select(p => p.Level).ToList();
        return new LoadResult(levels, skipped);
    }

    // ------------------------------------------------------------------
    //  Step 1: split the file into [Level] ... [End] blocks
    // ------------------------------------------------------------------

    private static List<RawLevel> ReadRawLevels(string path, string file, bool firstOnly)
    {
        var result = new List<RawLevel>();
        RawLevel? current = null;
        int lineNumber = 0;

        // 'using' closes the file even if an exception is thrown halfway through reading
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);

        string? text;
        while ((text = reader.ReadLine()) != null)
        {
            lineNumber++;
            string line = text.Trim();

            if (line.Length == 0 || line.StartsWith("//")) continue;

            if (line.Equals("[Level]", StringComparison.OrdinalIgnoreCase))
            {
                if (current != null)
                    throw new LevelFormatException(file, lineNumber, "found [Level] before the previous level's [End].");

                current = new RawLevel { StartLine = lineNumber };
            }
            else if (line.Equals("[End]", StringComparison.OrdinalIgnoreCase))
            {
                if (current == null)
                    throw new LevelFormatException(file, lineNumber, "found [End] without a [Level] before it.");

                result.Add(current);
                current = null;

                if (firstOnly) return result;
            }
            else
            {
                if (current == null)
                    throw new LevelFormatException(file, lineNumber, "this line is outside a [Level] ... [End] block.");

                current.Lines.Add((lineNumber, line));
            }
        }

        if (current != null)
            throw new LevelFormatException(file, current.StartLine, "this [Level] has no [End].");

        return result;
    }

    // ------------------------------------------------------------------
    //  Step 2: turn one block into a LevelData, checking everything
    // ------------------------------------------------------------------

    private static LevelData ParseLevel(RawLevel raw, string file, bool requireOrder, out int order)
    {
        LevelFormatException Fail(int line, string problem) => new LevelFormatException(file, line, problem);

        // --- Read the Key=Value lines, then the map rows after "Map:" ---
        var values = new Dictionary<string, (int Line, string Text)>(StringComparer.OrdinalIgnoreCase);
        var rows = new List<(int Line, string Text)>();
        bool inMap = false;

        foreach ((int lineNumber, string text) in raw.Lines)
        {
            if (inMap)
            {
                rows.Add((lineNumber, text));
                continue;
            }

            if (text.Equals("Map:", StringComparison.OrdinalIgnoreCase))
            {
                inMap = true;
                continue;
            }

            int equals = text.IndexOf('=');
            if (equals <= 0)
                throw Fail(lineNumber, "expected 'Key=Value' or 'Map:', but found \"" + text + "\".");

            string key = text.Substring(0, equals).Trim();
            string value = text.Substring(equals + 1).Trim();

            if (!KnownKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
                throw Fail(lineNumber, "unknown parameter '" + key + "'. Known parameters: " + string.Join(", ", KnownKeys) + ".");

            if (values.ContainsKey(key))
                throw Fail(lineNumber, "parameter '" + key + "' is given more than once.");

            values[key] = (lineNumber, value);
        }

        // --- Missing parameters: report all of them at once ---
        string label = values.TryGetValue("Name", out var nameEntry) && nameEntry.Text.Length > 0
            ? "level '" + nameEntry.Text + "'"
            : "this level";

        var missing = RequiredKeys.Where(k => !values.ContainsKey(k)).ToList();
        if (requireOrder && !values.ContainsKey("Order")) missing.Add("Order");
        if (!inMap || rows.Count == 0) missing.Add("Map");

        if (missing.Count > 0)
            throw Fail(raw.StartLine, label + " is missing required parameter(s): " + string.Join(", ", missing) + ".");

        if (values["Name"].Text.Length == 0)
            throw Fail(values["Name"].Line, "'Name' can't be empty.");

        // --- Numbers ---
        int ReadInt(string key, int min)
        {
            (int line, string text) = values[key];

            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
                throw Fail(line, label + ": '" + key + "' must be a whole number, but it is \"" + text + "\".");

            if (number < min)
                throw Fail(line, label + ": '" + key + "' must be at least " + min + ".");

            return number;
        }

        order = values.ContainsKey("Order") ? ReadInt("Order", int.MinValue) : int.MaxValue;
        int maxBlocks = ReadInt("MaxBlocks", 1);
        int moveForward = ReadInt("MoveForward", 0);
        int turnLeft = ReadInt("TurnLeft", 0);
        int turnRight = ReadInt("TurnRight", 0);
        int repeat = ReadInt("Repeat", 0);

        // Facing is a word, not a number
        (int facingLine, string facingText) = values["Facing"];
        Direction facing;
        switch (facingText.ToLowerInvariant())
        {
            case "up": facing = Direction.Up; break;
            case "right": facing = Direction.Right; break;
            case "down": facing = Direction.Down; break;
            case "left": facing = Direction.Left; break;
            default:
                throw Fail(facingLine, label + ": 'Facing' must be Up, Right, Down or Left, but it is \"" + facingText + "\".");
        }

        if (moveForward + turnLeft + turnRight + repeat == 0)
            throw Fail(raw.StartLine, label + " gives the player no blocks (all four counts are 0).");

        // --- Map ---
        int height = rows.Count;
        int width = rows[0].Text.Length;
        var map = new int[height, width];
        int starts = 0;
        int goals = 0;

        for (int y = 0; y < height; y++)
        {
            (int line, string text) = rows[y];

            if (text.Length != width)
                throw Fail(line, label + ": this map row has " + text.Length + " tiles but the first row has " + width + ". Every row must be the same length.");

            for (int x = 0; x < width; x++)
            {
                int tile = TileFromLetter(text[x]);
                if (tile < 0)
                    throw Fail(line, label + ": unknown tile '" + text[x] + "' in column " + (x + 1) + ". Use W (wall), F (floor), S (start), G (goal) or H (hazard).");

                if (tile == LevelTileMap.Start) starts++;
                if (tile == LevelTileMap.Goal) goals++;
                map[y, x] = tile;
            }
        }

        if (starts == 0)
            throw Fail(raw.StartLine, label + " has no start tile (S).");
        if (starts > 1)
            throw Fail(raw.StartLine, label + " has " + starts + " start tiles (S). It needs exactly one.");
        if (goals == 0)
            throw Fail(raw.StartLine, label + " has no goal tile (G).");

        return new LevelData(values["Name"].Text, map, maxBlocks, moveForward, turnLeft, turnRight, repeat, facing);
    }

    // Returns a tile ID from LevelTileMap, or -1 for a letter that isn't a tile
    private static int TileFromLetter(char letter)
    {
        switch (char.ToUpperInvariant(letter))
        {
            case 'W': return LevelTileMap.Wall;
            case 'F': return LevelTileMap.Floor;
            case 'S': return LevelTileMap.Start;
            case 'G': return LevelTileMap.Goal;
            case 'H': return LevelTileMap.Hazard;
            default: return -1;
        }
    }
}