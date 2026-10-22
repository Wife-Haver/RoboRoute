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

        /// <summary>True while the robot is still animating a move or a turn.</summary>
        public bool IsBusy => _isMoving || _isTurning;

        public Robot(Texture2D texture, Point startTile)
        {
            _texture = texture;
            Reset(startTile);
        }

        /// <summary>Puts the robot back on a tile, facing up, with no animation in progress.</summary>
        public void Reset(Point startTile)
        {
            TilePos = _targetTile = startTile;
            _pixelPos = TileToPixel(startTile);
            _facing = 0;
            _angle = _targetAngle = 0f;
            _isMoving = false;
            _isTurning = false;
        }

        private static Vector2 TileToPixel(Point tile) =>
            new Vector2(tile.X * TileSize, tile.Y * TileSize);

        // ------------------------------------------------------------------
        //  Commands (used by the code blocks). Only call these when !IsBusy.
        // ------------------------------------------------------------------

        public void TurnLeft()
        {
            _facing = (_facing + 3) % 4;            // counter-clockwise
            _targetAngle -= MathHelper.PiOver2;
            _isTurning = true;
        }

        public void TurnRight()
        {
            _facing = (_facing + 1) % 4;            // clockwise
            _targetAngle += MathHelper.PiOver2;
            _isTurning = true;
        }

        /// <summary>Moves one tile in the facing direction. Returns false if something is in the way.</summary>
        public bool MoveForward(Func<Point, bool> isWalkable)
        {
            return TryMove(_facing, isWalkable);
        }

        private bool TryMove(int directionIndex, Func<Point, bool> isWalkable)
        {
            Point dir = Directions[directionIndex];
            Point next = new Point(TilePos.X + dir.X, TilePos.Y + dir.Y);

            if (!isWalkable(next))
                return false;

            _targetTile = next;
            _isMoving = true;
            return true;
        }

        // ------------------------------------------------------------------
        //  Optional manual control (handy for testing the level)
        // ------------------------------------------------------------------

        public void HandleKeyboard(KeyboardState kb, Func<Point, bool> isWalkable)
        {
            // Only accept new input when idle (not mid-move, not mid-turn)
            if (IsBusy) return;

            if (kb.IsKeyDown(Keys.Left))
                TurnLeft();
            else if (kb.IsKeyDown(Keys.Right))
                TurnRight();
            else if (kb.IsKeyDown(Keys.Up))
                TryMove(_facing, isWalkable);
            else if (kb.IsKeyDown(Keys.Down))
                TryMove((_facing + 2) % 4, isWalkable);   // backward = opposite of facing
        }

        // ------------------------------------------------------------------
        //  Animation
        // ------------------------------------------------------------------

        public void Update(GameTime gameTime)
        {

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

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
        private const float TileFill = 0.8f;

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            Vector2 center = _pixelPos + new Vector2(TileSize / 2f, TileSize / 2f);

            // adjust sprite size
            float scale = 0.05f;//TileSize * TileFill / Math.Max(_texture.Width, _texture.Height);

            spriteBatch.Draw(
                _texture,
                center,
                null,
                Color.White,
                _angle,
                origin,
                scale,
                SpriteEffects.None,
                0f);
        }
    }
}