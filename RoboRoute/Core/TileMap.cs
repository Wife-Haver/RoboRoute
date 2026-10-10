using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoboRoute.RoboRoute.Core
{
    public class TileMap
    {
        public const int TileSize = 24;

        // Tile IDs
        public const int Floor = 0;
        public const int Wall = 1;

        private readonly int[,] _tiles;       // [row, column] = [y, x]
        private readonly Texture2D[] _textures; // index = tile ID

        public int Width => _tiles.GetLength(1);
        public int Height => _tiles.GetLength(0);

        public TileMap(int[,] tiles, Texture2D floorTexture, Texture2D wallTexture)
        {
            _tiles = tiles;
            _textures = new Texture2D[2];
            _textures[Floor] = floorTexture;
            _textures[Wall] = wallTexture;
        }

        public bool InBounds(Point tile) =>
            tile.X >= 0 && tile.Y >= 0 && tile.X < Width && tile.Y < Height;

        public bool IsWalkable(Point tile)
        {
            if (!InBounds(tile)) return false;
            return _tiles[tile.Y, tile.X] != Wall;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int id = _tiles[y, x];
                    Vector2 pos = new Vector2(x * TileSize, y * TileSize);
                    spriteBatch.Draw(_textures[id], pos, Color.White);
                }
            }
        }
    }
}
