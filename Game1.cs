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

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
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

            _tileMap = new TileMap(
                map,
                Content.Load<Texture2D>("FloorTile"),
                Content.Load<Texture2D>("WallTile"));

            _spriteBatch = new SpriteBatch(GraphicsDevice);

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
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here



            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _tileMap.Draw(_spriteBatch);
            robot.Draw(_spriteBatch);


            base.Draw(gameTime);
            _spriteBatch.End();
        }
    }
}
