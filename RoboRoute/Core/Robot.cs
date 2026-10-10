using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboRoute.RoboRoute.Core
{
    public class Robot
    {
        public const int TileSize = 24;

        public Point TilePos;              // logical cell
        private Vector2 _pixelPos;         // drawn position
        private Point _targetTile;
        private bool _isMoving;
        private float _speed = 160f;       // pixels per second

        private Texture2D _texture;

        public Robot(Texture2D texture, Point startTile)
        {
            _texture = texture;
            TilePos = _targetTile = startTile;
            _pixelPos = TileToPixel(startTile);
        }

        private static Vector2 TileToPixel(Point tile) =>
            new Vector2(tile.X * TileSize, tile.Y * TileSize);

        public void Update(GameTime gameTime, KeyboardState kb, Func<Point, bool> isWalkable)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!_isMoving)
            {
                Point dir = Point.Zero;

                // Cardinal directions only, one at a time (no diagonals)
                if (kb.IsKeyDown(Keys.Up) || kb.IsKeyDown(Keys.W)) dir = new Point(0, -1);
                else if (kb.IsKeyDown(Keys.Down) || kb.IsKeyDown(Keys.S)) dir = new Point(0, 1);
                else if (kb.IsKeyDown(Keys.Left) || kb.IsKeyDown(Keys.A)) dir = new Point(-1, 0);
                else if (kb.IsKeyDown(Keys.Right) || kb.IsKeyDown(Keys.D)) dir = new Point(1, 0);

                if (dir != Point.Zero)
                {
                    Point next = new Point(TilePos.X + dir.X, TilePos.Y + dir.Y);
                    if (isWalkable(next))
                    {
                        _targetTile = next;
                        _isMoving = true;
                    }
                }
            }

            if (_isMoving)
            {
                Vector2 targetPixel = TileToPixel(_targetTile);
                Vector2 toTarget = targetPixel - _pixelPos;
                float step = _speed * dt;

                if (toTarget.Length() <= step)
                {
                    // Arrived: snap exactly to the tile
                    _pixelPos = targetPixel;
                    TilePos = _targetTile;
                    _isMoving = false;
                }
                else
                {
                    toTarget.Normalize();
                    _pixelPos += toTarget * step;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _pixelPos, Color.White);
        }
    }
}
