using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace RoboRoute.RoboRoute.Core
{
    public class Robot
    {
        public const int TileSize = 24;

        public Point TilePos;              // logical cell
        private Vector2 _pixelPos;         // drawn position (top-left of tile)
        private Point _targetTile;
        private bool _isMoving;
        private float _speed = 160f;       // pixels per second

        // Facing: 0 = up, 1 = right, 2 = down, 3 = left
        private int _facing = 0;
        private static readonly Point[] Directions =
        {
            new Point(0, -1),  // up
            new Point(1, 0),   // right
            new Point(0, 1),   // down
            new Point(-1, 0),  // left
        };

        private float _angle;              // current drawn rotation (radians)
        private float _targetAngle;        // where the rotation is heading
        private bool _isTurning;
        private float _turnSpeed = MathHelper.Pi * 4f; // radians per second (90° in ~0.125s)

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

            // Only accept new input when idle (not mid-move, not mid-turn)
            if (!_isMoving && !_isTurning)
            {
                if (kb.IsKeyDown(Keys.Left))
                {
                    _facing = (_facing + 3) % 4;            // turn counter-clockwise
                    _targetAngle -= MathHelper.PiOver2;
                    _isTurning = true;
                }
                else if (kb.IsKeyDown(Keys.Right))
                {
                    _facing = (_facing + 1) % 4;            // turn clockwise
                    _targetAngle += MathHelper.PiOver2;
                    _isTurning = true;
                }
                else if (kb.IsKeyDown(Keys.Up) || kb.IsKeyDown(Keys.Down))
                {
                    // Forward uses the facing direction, backward uses the opposite
                    int moveDir = kb.IsKeyDown(Keys.Up) ? _facing : (_facing + 2) % 4;
                    Point dir = Directions[moveDir];
                    Point next = new Point(TilePos.X + dir.X, TilePos.Y + dir.Y);

                    if (isWalkable(next))
                    {
                        _targetTile = next;
                        _isMoving = true;
                    }
                }
            }

            // Animate turning
            if (_isTurning)
            {
                float diff = _targetAngle - _angle;
                float step = _turnSpeed * dt;

                if (Math.Abs(diff) <= step)
                {
                    _angle = _targetAngle;
                    _isTurning = false;
                }
                else
                {
                    _angle += Math.Sign(diff) * step;
                }
            }

            // Animate moving
            if (_isMoving)
            {
                Vector2 targetPixel = TileToPixel(_targetTile);
                Vector2 toTarget = targetPixel - _pixelPos;
                float step = _speed * dt;

                if (toTarget.Length() <= step)
                {
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
            // Rotate around the sprite's center, so draw at the tile's center
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            Vector2 center = _pixelPos + new Vector2(TileSize / 2f, TileSize / 2f);

            spriteBatch.Draw(
                _texture,
                center,
                null,
                Color.White,
                _angle,
                origin,
                1f,
                SpriteEffects.None,
                0f);
        }
    }
}