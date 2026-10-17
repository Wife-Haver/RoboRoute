using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RoboRoute.RoboRoute.Core
{
    public class LevelTileMap
    {
        public const int TileSize = 24;

        // Tile IDs
        public const int Floor = 0;
        public const int Wall = 1;
        public const int Start = 2;
        public const int Goal = 3;

        // Content file name for each tile ID (index = tile ID).
        // To add a tile type: add an ID constant above and its texture name here.
        private static readonly string[] TextureNames =
        {
            "FloorTile",   // Floor
            "WallTile",    // Wall
            "StartTile",   // Start
            "GoalTile",    // Goal
        };

        private readonly int[,] _tiles;         // [row, column] = [y, x]
        private readonly Texture2D[] _textures; // index = tile ID

        public int Width => _tiles.GetLength(1);
        public int Height => _tiles.GetLength(0);

        // The map loads its own textures, so Game1 doesn't have to
        public LevelTileMap(int[,] tiles, ContentManager content)
        {
            _tiles = tiles;

            _textures = new Texture2D[TextureNames.Length];
            for (int i = 0; i < TextureNames.Length; i++)
                _textures[i] = content.Load<Texture2D>(TextureNames[i]);
        }

        public bool InBounds(Point tile) =>
            tile.X >= 0 && tile.Y >= 0 && tile.X < Width && tile.Y < Height;

        // Start and Goal are walkable; only walls (and off-map) block the robot
        public bool IsWalkable(Point tile)
        {
            if (!InBounds(tile)) return false;
            return _tiles[tile.Y, tile.X] != Wall;
        }

        public bool IsGoal(Point tile) =>
            InBounds(tile) && _tiles[tile.Y, tile.X] == Goal;

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

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int id = _tiles[y, x];
                    Vector2 pos = new Vector2(x * TileSize, y * TileSize);

                    // Start and Goal sit on top of a floor tile, so their art can have transparent edges
                    if (id == Start || id == Goal)
                        spriteBatch.Draw(_textures[Floor], pos, Color.White);

                    spriteBatch.Draw(_textures[id], pos, Color.White);
                }
            }
        }
    }
}