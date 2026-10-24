using System.Drawing;

namespace RoboRoute.Winforms.Core
{
    // The tile grid and the rules about it. It holds no images and draws nothing:
    // the level panel (in Controls) looks up an image for each tile ID and paints the map.
    public class LevelTileMap
    {
        // Size of one tile in "map units". The level panel scales these up to fit the window.
        public const int TileSize = 24;

        // Tile IDs
        public const int Floor = 0;
        public const int Wall = 1;
        public const int Start = 2;
        public const int Goal = 3;
        public const int Hazard = 4;

        private readonly int[,] _tiles;         // [row, column] = [y, x]

        public int Width => _tiles.GetLength(1);
        public int Height => _tiles.GetLength(0);

        public LevelTileMap(int[,] tiles)
        {
            _tiles = tiles;
        }

        // Returns the tile ID at a position, or -1 if it's off the map
        public int GetTile(Point tile) =>
            InBounds(tile) ? _tiles[tile.Y, tile.X] : -1;

        public bool InBounds(Point tile) =>
            tile.X >= 0 && tile.Y >= 0 && tile.X < Width && tile.Y < Height;

        // Start, Goal and Hazard are walkable; only walls (and off-map) block the robot
        public bool IsWalkable(Point tile)
        {
            if (!InBounds(tile)) return false;
            return _tiles[tile.Y, tile.X] != Wall;
        }

        public bool IsGoal(Point tile) =>
            InBounds(tile) && _tiles[tile.Y, tile.X] == Goal;

        public bool IsHazard(Point tile) =>
            InBounds(tile) && _tiles[tile.Y, tile.X] == Hazard;

        // Returns the first tile with this ID, or null if the map has none.
        // Handy for finding where the robot should start: FindTile(LevelTileMap.Start)
        public Point? FindTile(int id)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (_tiles[y, x] == id)
                        return new Point(x, y);
                }
            }
            return null;
        }
    }
}
