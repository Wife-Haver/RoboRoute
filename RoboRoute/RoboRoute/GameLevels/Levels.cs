using RoboRoute.RoboRoute.GameLevels;

namespace RoboRoute.RoboRoute.Core
{
    // The list of all levels, in play order. To add a level: write a method like Level1() below
    // and add it to the All array.
    public static class Levels
    {
        // Short names so the maps are easy to read and edit
        private const int F = LevelTileMap.Floor;   // floor
        private const int W = LevelTileMap.Wall;    // wall
        private const int S = LevelTileMap.Start;   // where the robot starts (it begins facing UP)
        private const int G = LevelTileMap.Goal;    // where the program must end

        private const int H = LevelTileMap.Hazard;

        public static readonly LevelData[] All =
        {
            
            Level1(),
            Level2(),
            Level3(),


        };

        // Teaches: Move Forward and Turn Right.
        // Intended solution: Move, Move, Turn Right, Move, Move, Move, Move   (7 blocks)
        private static LevelData Level1()
        {
            int[,] map =
            {
                { W, W, W, W, W, W, W },
                { W, F, F, F, F, G, W },
                { W, F, W, W, W, W, W },
                { W, S, W, W, W, W, W },
                { W, W, W, W, W, W, W },
            };

            return new LevelData(
                name: "First Steps",
                map: map,
                maxBlocksAllowed: 8,
                moveForwardBlocks: 7,
                turnLeftBlocks: 0,       // not available in this level
                turnRightBlocks: 2,
                repeatBlocks: 0);        // not available in this level
        }

        // Teaches: Repeat. A staircase going up and to the right, four steps.
        // Without a loop it needs 15 blocks, but only 6 are allowed, so the player has to use Repeat.
        // Intended solution: Repeat 4 [ Move, Turn Right, Move, Turn Left ]   (5 blocks)
        private static LevelData Level2()
        {
            int[,] map =
            {
                { W, W, W, W, W, W, W, W },
                { W, W, W, W, F, G, W, W },
                { W, W, W, F, F, W, W, W },
                { W, W, F, F, W, W, W, W },
                { W, F, F, W, W, W, W, W },
                { W, S, W, W, W, W, W, W },
                { W, W, W, W, W, W, W, W },
            };

            return new LevelData(
                name: "Staircase",
                map: map,
                maxBlocksAllowed: 6,
                moveForwardBlocks: 3,
                turnLeftBlocks: 2,
                turnRightBlocks: 2,
                repeatBlocks: 1);
        }

        private static LevelData Level3()
        {
            int[,] map =
            {
                { W, W, W, W, W, W, W },
                { W, H, F, F, F, H, W },
                { W, F, F, H, F, F, W },
                { W, S, F, H, F, G, W },
                { W, F, F, H, F, F, W },
                { W, H, F, F, F, H, W },
                { W, W, W, W, W, W, W }
            };

            return new LevelData(
                name: "Hazardous",
                map: map,
                maxBlocksAllowed: 14,
                moveForwardBlocks: 9,
                turnLeftBlocks: 3,       // not available in this level
                turnRightBlocks: 4,
                repeatBlocks: 0);        // not available in this level
        }
    }
}