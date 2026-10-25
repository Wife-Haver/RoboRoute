namespace RoboRoute.Winforms.Core;

// The kinds of code blocks. Each kind has its own Block subclass (see Block.Create).
// Levels use this to say how many of each block the player is given.
public enum BlockType
{
    MoveForward,
    TurnLeft,
    TurnRight,
    Repeat
}
