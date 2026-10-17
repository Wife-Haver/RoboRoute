using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoboRoute.RoboRoute.Core
{
    // Draws the level (tile map + robot) at a low internal resolution,
    // then scales it up to fit inside the level viewport.
    public class LevelView
    {
        public const int VirtualWidth = 320;   // game's internal resolution
        public const int VirtualHeight = 180;  // (16:9, fits ~13x7 tiles of 24px)

        private readonly RenderTarget2D _target;
        private readonly LevelTileMap _tileMap;
        private readonly Robot _robot;

        public LevelView(GraphicsDevice graphicsDevice, LevelTileMap tileMap, Robot robot)
        {
            _tileMap = tileMap;
            _robot = robot;
            _target = new RenderTarget2D(graphicsDevice, VirtualWidth, VirtualHeight);
        }

        // Step 1: draw the level into the low-res render target.
        // Leaves the graphics device pointing back at the backbuffer when done.
        public void RenderToTarget(GraphicsDevice graphicsDevice, SpriteBatch sb)
        {
            graphicsDevice.SetRenderTarget(_target);
            graphicsDevice.Clear(Color.CornflowerBlue);

            int offsetX = 1;
            int offsetY = (VirtualHeight - _tileMap.Height * LevelTileMap.TileSize) / 2;

            sb.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateTranslation(offsetX, offsetY, 0));

            _tileMap.Draw(sb);
            _robot.Draw(sb);

            sb.End();

            graphicsDevice.SetRenderTarget(null);
        }

        // Step 2: draw the render target, scaled and centred, inside the given viewport
        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch sb, Viewport viewport)
        {
            graphicsDevice.Viewport = viewport;

            float scale = Math.Min((float)viewport.Width / VirtualWidth,
                                   (float)viewport.Height / VirtualHeight);

            int destW = (int)(VirtualWidth * scale);
            int destH = (int)(VirtualHeight * scale);

            // Coordinates are relative to the viewport, not the window
            Rectangle dest = new Rectangle(
                (viewport.Width - destW) / 2,
                (viewport.Height - destH) / 2,
                destW, destH);

            sb.Begin(samplerState: SamplerState.PointClamp);
            sb.Draw(_target, dest, Color.White);
            sb.End();
        }
    }
}