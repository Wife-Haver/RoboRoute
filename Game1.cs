using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoboRoute.RoboRoute.Core;



namespace RoboRoute
{
    public class Game1 : Game

    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private LevelTileMap _LevelTileMap;

        private Robot robot;
        private const int VirtualWidth = 320;   // game's internal resolution
        private const int VirtualHeight = 320;  // (16:16, fits ~13x13 tiles of 24px)
        private RenderTarget2D _renderTarget;

        // Fraction of the window WIDTH given to the level (left viewport)
        private float _levelRatio = 0.5f;
        private const float MinLevelRatio = 0.5f;   // level always >= 50%, so it's always the biggest
        private const float MaxLevelRatio = 0.7f;

        // How the REMAINING width is split between the middle and right viewports (0..1 = share for middle)
        private float _middleShare = 0.5f;
        private const float MinMiddleShare = 0.2f;
        private const float MaxMiddleShare = 0.8f;

        private Viewport _levelViewport;
        private Viewport _middleViewport;
        private Viewport _rightViewport;

        private Texture2D _pixel; // 1x1 white texture for drawing UI rectangles

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferredBackBufferWidth = 1280;   // actual window size
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (s, e) => UpdateViewports();
        }

        private void UpdateViewports()
        {
            // The event can fire before the graphics device exists
            if (GraphicsDevice == null) return;

            int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int h = GraphicsDevice.PresentationParameters.BackBufferHeight;

            _levelRatio = MathHelper.Clamp(_levelRatio, MinLevelRatio, MaxLevelRatio);
            _middleShare = MathHelper.Clamp(_middleShare, MinMiddleShare, MaxMiddleShare);

            // Work in whole pixels so the three viewports always add up to the window width exactly
            int levelWidth = (int)(w * _levelRatio);
            int remaining = w - levelWidth;
            int middleWidth = (int)(remaining * _middleShare);
            int rightWidth = remaining - middleWidth;

            _levelViewport = new Viewport(0, 0, levelWidth, h);
            _middleViewport = new Viewport(levelWidth, 0, middleWidth, h);
            _rightViewport = new Viewport(levelWidth + middleWidth, 0, rightWidth, h);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            int[,] map = new int[,]
                {
                    {1,1,1,1,1,1,1,1},
                    {1,0,0,0,0,0,0,1},
                    {1,0,0,1,0,0,0,1},
                    {1,0,0,0,0,0,0,1},
                    {1,1,1,1,1,1,1,1},
                };

            _LevelTileMap = new LevelTileMap(map, Content.Load<Texture2D>("FloorTile"), Content.Load<Texture2D>("WallTile"));

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _renderTarget = new RenderTarget2D(GraphicsDevice, VirtualWidth, VirtualHeight);

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            robot = new Robot(Content.Load<Texture2D>("RobotProto"), new Point(1, 1));

            UpdateViewports();
        }

        protected override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || kb.IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // [ and ] : shrink / grow the level viewport
            if (kb.IsKeyDown(Keys.OemOpenBrackets)) { _levelRatio -= 0.3f * dt; UpdateViewports(); }
            if (kb.IsKeyDown(Keys.OemCloseBrackets)) { _levelRatio += 0.3f * dt; UpdateViewports(); }

            // , and . : move the divider between the middle and right viewports
            if (kb.IsKeyDown(Keys.OemComma)) { _middleShare -= 0.5f * dt; UpdateViewports(); }
            if (kb.IsKeyDown(Keys.OemPeriod)) { _middleShare += 0.5f * dt; UpdateViewports(); }

            robot.Update(gameTime, kb, _LevelTileMap.IsWalkable);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // 1. Draw the game at low resolution into the render target
            //    (SetRenderTarget resets the viewport to the target's size, which is what we want here)
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            int offsetX = 1;
            int offsetY = (VirtualHeight - _LevelTileMap.Height * LevelTileMap.TileSize) / 2;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateTranslation(offsetX, offsetY, 0));

            _LevelTileMap.Draw(_spriteBatch);
            robot.Draw(_spriteBatch);

            _spriteBatch.End();

            // 2. Back to the window. Clear the WHOLE backbuffer first (Clear ignores the viewport on some platforms)
            GraphicsDevice.SetRenderTarget(null);

            int winW = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int winH = GraphicsDevice.PresentationParameters.BackBufferHeight;

            GraphicsDevice.Viewport = new Viewport(0, 0, winW, winH);
            GraphicsDevice.Clear(Color.Black);

            // 3. LEVEL viewport: scale the render target up to fit inside it
            GraphicsDevice.Viewport = _levelViewport;

            float scale = Math.Min((float)_levelViewport.Width / VirtualWidth,
                                   (float)_levelViewport.Height / VirtualHeight);

            int destW = (int)(VirtualWidth * scale);
            int destH = (int)(VirtualHeight * scale);

            Rectangle dest = new Rectangle(
                (_levelViewport.Width - destW) / 2,
                (_levelViewport.Height - destH) / 2,
                destW, destH);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_renderTarget, dest, Color.White);
            _spriteBatch.End();

            // 4. MIDDLE viewport: (0,0) is its own top-left corner
            GraphicsDevice.Viewport = _middleViewport;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawMiddlePanel(_spriteBatch);
            _spriteBatch.End();

            // 5. RIGHT viewport
            GraphicsDevice.Viewport = _rightViewport;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawRightPanel(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawMiddlePanel(SpriteBatch sb)
        {
            // Panel background
            sb.Draw(_pixel, new Rectangle(0, 0, _middleViewport.Width, _middleViewport.Height), new Color(30, 30, 40));

            // Divider line on the left edge
            sb.Draw(_pixel, new Rectangle(0, 0, 2, _middleViewport.Height), Color.White);

            // TODO: draw the middle panel's content here
        }

        private void DrawRightPanel(SpriteBatch sb)
        {
            // Panel background (slightly different shade so you can tell them apart)
            sb.Draw(_pixel, new Rectangle(0, 0, _rightViewport.Width, _rightViewport.Height), new Color(45, 35, 55));

            // Divider line on the left edge
            sb.Draw(_pixel, new Rectangle(0, 0, 2, _rightViewport.Height), Color.White);

            // TODO: draw the right panel's content here
        }
    }
}