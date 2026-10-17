using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoboRoute.RoboRoute.Content;
using RoboRoute.RoboRoute.Core;

namespace RoboRoute
{
    // Game1 only wires things together: load content, then call Update/Draw on each piece.
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private LevelTileMap _tileMap;
        private Robot _robot;

        private ProgramRunner _runner;       // the player's program + running it
        private ViewportLayout _layout;      // the three viewports
        private LevelView _levelView;        // draws the level into the left viewport
        private CodeEditorUI _editorUI;      // middle + right panels, drag and drop

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferredBackBufferWidth = 1280;   // actual window size
            _graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            LevelData level = LevelData.Level1();

            _tileMap = new LevelTileMap(level.Map,Content);
            Point startTile = _tileMap.FindTile(LevelTileMap.Start) ?? new Point(1, 1); ;

            _robot = new Robot(Content.Load<Texture2D>("RobotProto"), startTile);

            _runner = new ProgramRunner(_robot, _tileMap.IsWalkable, startTile); //level.RobotStart

            _layout = new ViewportLayout();
            SyncLayoutToWindow();

            _levelView = new LevelView(GraphicsDevice, _tileMap, _robot);

            var ui = new UiDraw(GraphicsDevice, Content.Load<SpriteFont>("Font"));
            _editorUI = new CodeEditorUI(_layout, _runner, ui);
        }

        protected override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || kb.IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            SyncLayoutToWindow();
            _layout.Update(kb, dt);
            _editorUI.Update(Mouse.GetState());

            // Manual arrow-key control, only while no program is running.
            // Delete this line once you don't want the player moving the robot directly.
            if (!_runner.IsRunning)
                _robot.HandleKeyboard(kb, _tileMap.IsWalkable);

            _robot.Update(gameTime);
            _runner.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Draw the level into its low-res render target first
            _levelView.RenderToTarget(GraphicsDevice, _spriteBatch);

            // Clear the WHOLE backbuffer (Clear ignores the viewport on some platforms)
            GraphicsDevice.Viewport = _layout.Window;
            GraphicsDevice.Clear(Color.Black);

            _levelView.Draw(GraphicsDevice, _spriteBatch, _layout.Level);
            _editorUI.Draw(GraphicsDevice, _spriteBatch);

            base.Draw(gameTime);
        }

        // Keeps the viewports in step with the window size (only recalculates when it changes)
        private void SyncLayoutToWindow()
        {
            var pp = GraphicsDevice.PresentationParameters;
            _layout.SetWindowSize(pp.BackBufferWidth, pp.BackBufferHeight);
        }
    }
}