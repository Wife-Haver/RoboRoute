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

        private TileMap _tileMap;

        private Robot robot;
        private const int VirtualWidth = 320;   // game's internal resolution
        private const int VirtualHeight = 180;  // (16:9, fits ~13x7 tiles of 24px)
        private RenderTarget2D _renderTarget;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;   // actual window size
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            

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

            _tileMap = new TileMap(map,Content.Load<Texture2D>("FloorTile"),Content.Load<Texture2D>("WallTile"));

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _renderTarget = new RenderTarget2D(GraphicsDevice, VirtualWidth, VirtualHeight);

            robot = new Robot(Content.Load<Texture2D>("RobotProto"), new Point(1, 1));

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            robot.Update(gameTime, Keyboard.GetState(), _tileMap.IsWalkable);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // 1. Draw the game at low resolution into the render target
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            int offsetX = 1; //(VirtualWidth - _tileMap.Width * TileMap.TileSize) / 2
            int offsetY = (VirtualHeight - _tileMap.Height * TileMap.TileSize) / 2;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateTranslation(offsetX, offsetY, 0));
            _tileMap.Draw(_spriteBatch);
            robot.Draw(_spriteBatch);
            _spriteBatch.End();

            // 2. Draw the render target to the window, scaled up
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            int winW = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int winH = GraphicsDevice.PresentationParameters.BackBufferHeight;

            // Whole-number scale keeps pixels perfectly even
            int scale = Math.Max(1, Math.Min(winW / VirtualWidth, winH / VirtualHeight));

            Rectangle dest = new Rectangle(
                (winW - VirtualWidth * scale) / 2,    // center it, leaving black bars
                (winH - VirtualHeight * scale) / 2,
                VirtualWidth * scale,
                VirtualHeight * scale);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_renderTarget, dest, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
