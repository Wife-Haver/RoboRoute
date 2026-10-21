using System;

using Microsoft.Xna.Framework;
using RoboRoute.RoboRoute.Commands;

namespace RoboRoute.RoboRoute.Core
{
    // Everything that describes one level. It's plain data: the actual levels are listed in Levels.cs.
    public class LevelData
    {
        public string Name { get; }

        // The tile map, [row, column]. Uses the IDs from LevelTileMap: Floor, Wall, Start, Goal
        public int[,] Map { get; }

        // Most blocks the player may place in total (blocks inside a Repeat count too)
        public int MaxBlocksAllowed { get; }

        // How many of each block the player is GIVEN. 0 means that block isn't available in this level.
        public int MoveForwardBlocks { get; }
        public int TurnLeftBlocks { get; }
        public int TurnRightBlocks { get; }
        public int RepeatBlocks { get; }

        // Found automatically from the map's Start tile, so the map is the only place that says where the robot begins
        public Point RobotStart { get; }

        public LevelData(
            string name,
            int[,] map,
            int maxBlocksAllowed,
            int moveForwardBlocks,
            int turnLeftBlocks,
            int turnRightBlocks,
            int repeatBlocks)
        {
            Name = name;
            Map = map;
            MaxBlocksAllowed = maxBlocksAllowed;
            MoveForwardBlocks = moveForwardBlocks;
            TurnLeftBlocks = turnLeftBlocks;
            TurnRightBlocks = turnRightBlocks;
            RepeatBlocks = repeatBlocks;

            RobotStart = FindStart(name, map);
        }

        // How many blocks of this type the player is given (lets the UI ask without a switch of its own)
        public int GetGivenCount(BlockType type)
        {
            switch (type)
            {
                case BlockType.MoveForward: return MoveForwardBlocks;
                case BlockType.TurnLeft:    return TurnLeftBlocks;
                case BlockType.TurnRight:   return TurnRightBlocks;
                case BlockType.Repeat:      return RepeatBlocks;
                default:                    return 0;
            }
        }

        private static Point FindStart(string name, int[,] map)
        {
            for (int y = 0; y < map.GetLength(0); y++)
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    if (map[y, x] == LevelTileMap.Start)
                        return new Point(x, y);
                }
            }

            // Fail loudly so a typo in a map is caught the moment the level loads
            throw new ArgumentException("Level '" + name + "' has no Start tile.");
        }
    }
}