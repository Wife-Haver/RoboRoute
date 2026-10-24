using System.Drawing;

namespace RoboRoute.Winforms.Core
{
    // The kinds of code blocks the player can drag into the queue.
    // Add new ones here, then add them to the palette in the main form.
    public enum BlockType
    {
        MoveForward,
        TurnLeft,
        TurnRight,
        Repeat
    }

    public static class BlockInfo
    {
        public static string GetLabel(BlockType type)
        {
            switch (type)
            {
                case BlockType.MoveForward: return "Move Forward";
                case BlockType.TurnLeft:    return "Turn Left";
                case BlockType.TurnRight:   return "Turn Right";
                case BlockType.Repeat:      return "Repeat";
                default:                    return type.ToString();
            }
        }

        public static Color GetColor(BlockType type)
        {
            switch (type)
            {
                case BlockType.MoveForward: return Color.FromArgb(46, 125, 80);
                case BlockType.TurnLeft:    return Color.FromArgb(52, 101, 164);
                case BlockType.TurnRight:   return Color.FromArgb(164, 101, 52);
                case BlockType.Repeat:      return Color.FromArgb(130, 70, 160);
                default:                    return Color.Gray;
            }
        }
    }
}
