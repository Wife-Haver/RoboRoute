namespace RoboRoute.Winforms.Core;

// Thrown by LevelParser when a level file has something wrong in it (a missing parameter, an unknown tile,
// a map with no Start...). It is our own exception type so the rest of the program can catch exactly
// "this level file is bad" without also catching unrelated bugs.
//
// The message already reads "file.txt, line 12: <what is wrong>", ready to show to the player.
public class LevelFormatException : Exception
{
    public string FileName { get; }

    // 1-based line in the file, or 0 when the problem is about the whole file
    public int LineNumber { get; }

    public LevelFormatException(string fileName, int lineNumber, string problem)
        : base(lineNumber > 0 ? fileName + ", line " + lineNumber + ": " + problem : fileName + ": " + problem)
    {
        FileName = fileName;
        LineNumber = lineNumber;
    }
}
