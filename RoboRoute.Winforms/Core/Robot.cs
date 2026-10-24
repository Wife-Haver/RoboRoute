using System;
using System.Drawing;
using System.Numerics;

namespace RoboRoute.Winforms.Core
{
    // The robot's logic and animation state. It draws nothing:
    // the level panel reads PixelPos and Angle and paints the sprite.
    public class Robot
    {
        private const float MoveSpeed = 160f;                  // map units per second
        private const float TurnSpeed = MathF.PI * 4f;         // radians per second (90 degrees in about 0.125s)

        private Vector2 _pixelPos;         // drawn position (top-left of the tile), in map units
        private Point _targetTile;
        private bool _isMoving;

        // Facing: 0 = up, 1 = right, 2 = down, 3 = left
        private int _facing = 0;
        private static readonly Point[] Directions =
        {
            new Point(0, -1),  // up
            new Point(1, 0),   // right
            new Point(0, 1),   // down
            new Point(-1, 0),  // left
        };

        private float _angle;              // current drawn rotation (radians, clockwise)
        private float _targetAngle;        // where the rotation is heading
        private bool _isTurning;

        // The logical cell the robot is on (updates when a move finishes)
        public Point TilePos { get; private set; }

        // Where to draw it: top-left of its tile in map units (24 per tile), and how far it's rotated
        public Vector2 PixelPos => _pixelPos;
        public float Angle => _angle;

        /// <summary>True while the robot is still animating a move or a turn.</summary>
        public bool IsBusy => _isMoving || _isTurning;

        public Robot(Point startTile)
        {
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
            new Vector2(tile.X * LevelTileMap.TileSize, tile.Y * LevelTileMap.TileSize);

        // ------------------------------------------------------------------
        //  Commands (used by the code blocks). Only call these when !IsBusy.
        // ------------------------------------------------------------------

        public void TurnLeft()
        {
            _facing = (_facing + 3) % 4;            // counter-clockwise
            _targetAngle -= MathF.PI / 2f;
            _isTurning = true;
        }

        public void TurnRight()
        {
            _facing = (_facing + 1) % 4;            // clockwise
            _targetAngle += MathF.PI / 2f;
            _isTurning = true;
        }

        /// <summary>Moves one tile in the facing direction. Returns false if something is in the way.</summary>
        public bool MoveForward(Func<Point, bool> isWalkable)
        {
            Point dir = Directions[_facing];
            Point next = new Point(TilePos.X + dir.X, TilePos.Y + dir.Y);

            if (!isWalkable(next))
                return false;

            _targetTile = next;
            _isMoving = true;
            return true;
        }

        // ------------------------------------------------------------------
        //  Animation (call every frame with the seconds since the last frame)
        // ------------------------------------------------------------------

        public void Update(float dt)
        {
            // Animate turning
            if (_isTurning)
            {
                float diff = _targetAngle - _angle;
                float step = TurnSpeed * dt;

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
                float step = MoveSpeed * dt;

                if (toTarget.Length() <= step)
                {
                    _pixelPos = targetPixel;
                    TilePos = _targetTile;
                    _isMoving = false;
                }
                else
                {
                    _pixelPos += Vector2.Normalize(toTarget) * step;
                }
            }
        }
    }
}
