using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoboRoute.RoboRoute.Core;

// If the compiler can't find UiDraw, ViewportLayout or LevelView, add a "using" line above
// for whichever namespace you put those files in.

namespace RoboRoute
{
    // Game1 only wires things together: load content, then call Update/Draw on each piece.
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // These stay the same for the whole game
        private ViewportLayout _layout;
        private UiDraw _ui;
        private LevelCompletePopup _popup;

        // These are rebuilt for every level (see LoadLevel)
        private int _levelIndex;
        private LevelTileMap _tileMap;
        private Robot _robot;
        private ProgramRunner _runner;
        private LevelView _levelView;
        private CodeEditorUI _editorUI;

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

            _ui = new UiDraw(GraphicsDevice, Content.Load<SpriteFont>("Font"));

            _layout = new ViewportLayout();
            SyncLayoutToWindow();

            _popup = new LevelCompletePopup(_layout, _ui);

            LoadLevel(0);
        }

        // Builds everything that belongs to one level. Also used to restart a level:
        // it throws away the old queue, robot and map and starts fresh.
        private void LoadLevel(int index)
        {
            _levelIndex = index;
            LevelData level = Levels.All[index];

            _tileMap = new LevelTileMap(level.Map, Content);
            _robot = new Robot(Content.Load<Texture2D>("RobotProto"), level.RobotStart);
            _runner = new ProgramRunner(_robot, _tileMap.IsWalkable, _tileMap.IsGoal, level.RobotStart);

            _levelView = new LevelView(GraphicsDevice, _tileMap, _robot);
            _editorUI = new CodeEditorUI(_layout, _runner, _ui, level);
        }

        protected override void Update(GameTime gameTime)
        {
            var kb = Keyboard.GetState();
            var mouse = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || kb.IsKeyDown(Keys.Escape))
                Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            SyncLayoutToWindow();
            _layout.Update(kb, dt);

            // The popup is modal: while it's open, nothing else in the game gets any input
            HandlePopupAction(_popup.Update(mouse, dt));
            if (_popup.IsVisible)
            {
                base.Update(gameTime);
                return;
            }

            _editorUI.Update(mouse, dt);

            // Manual arrow-key control, only while no program is running.
            // Delete this line once you don't want the player moving the robot directly.
            if (!_runner.IsRunning)
                _robot.HandleKeyboard(kb, _tileMap.IsWalkable);

            _robot.Update(gameTime);
            _runner.Update();

            // The program ended on the goal: open the "Level Done" window
            if (_runner.ReachedGoal)
            {
                _popup.Show(
                    Levels.All[_levelIndex].Name,
                    hasNextLevel: _levelIndex + 1 < Levels.All.Length,
                    mainMenuAvailable: false);   // set to true once the main menu exists
            }

            base.Update(gameTime);
        }

        private void HandlePopupAction(PopupAction action)
        {
            switch (action)
            {
                case PopupAction.Restart:
                    LoadLevel(_levelIndex);
                    break;

                case PopupAction.NextLevel:
                    LoadLevel(_levelIndex + 1);   // only offered when there is a next level
                    break;

                case PopupAction.MainMenu:
                    // TODO: switch to the main menu screen once it exists
                    break;
            }
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

            // Drawn last so it sits on top of everything (does nothing unless it's open)
            _popup.Draw(GraphicsDevice, _spriteBatch);

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