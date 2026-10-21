using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RoboRoute.RoboRoute.Commands
{
    // The kinds of code blocks the player can drag into the queue.
    // Add new ones here, then add them to the _palette array in Game1.
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
                case BlockType.TurnLeft: return "Turn Left";
                case BlockType.TurnRight: return "Turn Right";
                case BlockType.Repeat: return "Repeat";
                default: return type.ToString();
            }
        }

        public static Color GetColor(BlockType type)
        {
            switch (type)
            {
                case BlockType.MoveForward: return new Color(46, 125, 80);
                case BlockType.TurnLeft: return new Color(52, 101, 164);
                case BlockType.TurnRight: return new Color(164, 101, 52);
                case BlockType.Repeat: return new Color(130, 70, 160);

                default: return Color.Gray;
            }
        }
    }

}
