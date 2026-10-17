using Microsoft.Xna.Framework;

namespace RoboRoute.RoboRoute.Content
{
    // Everything that describes one level. Add more levels as extra static methods
    // (or later, load them from a file) instead of hardcoding them in Game1.
    public class LevelData
    {
        public int[,] Map { get; }
        public Point RobotStart { get; }

        public LevelData(int[,] map, Point robotStart)
        {
            Map = map;
            RobotStart = robotStart;
        }

        public static LevelData Level1()
        {
            int[,] map = new int[,]
            {
                {1,1,1,1,1,1,1,1},
                {1,0,0,0,0,0,0,1},
                {1,2,0,0,1,0,3,1},
                {1,0,0,0,0,0,0,1},
                {1,1,1,1,1,1,1,1},
            };

            return new LevelData(map, new Point(1, 1));
        }
    }
}