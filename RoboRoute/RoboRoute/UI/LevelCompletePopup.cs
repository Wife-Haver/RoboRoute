using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RoboRoute.RoboRoute.Core
{
    // What the player chose in the popup
    public enum PopupAction
    {
        None,
        Restart,
        MainMenu,
        NextLevel
    }

    // The "Level Done!" window: dims the screen and offers Restart / Main Menu / Next Level.
    // Game1 calls Show() when the goal is reached, then Update() every frame and acts on what it returns.
    public class LevelCompletePopup
    {
        private const float ShowDelay = 0.6f;       // seconds to let the player see the robot on the goal first
        private const int PanelMaxWidth = 460;
        private const int PanelHeight = 220;
        private const int PanelPadding = 20;
        private const int ButtonHeight = 50;
        private const int ButtonGap = 12;

        // Button order: 0 = Restart, 1 = Main Menu, 2 = Next Level
        private static readonly string[] Labels = { "Restart", "Main Menu", "Next Level" };
        private static readonly Color[] ButtonColors =
        {
            new Color(60, 85, 130),    // Restart
            new Color(90, 90, 105),    // Main Menu
            new Color(46, 140, 80),    // Next Level
        };

        private readonly ViewportLayout _layout;
        private readonly UiDraw _ui;

        private bool _visible;
        private float _delay;               // counts down after Show() before the window appears
        private bool _waitForRelease;       // ignore a click that was already in progress when it opened
        private string _levelName = "";
        private bool _hasNextLevel;
        private bool _mainMenuAvailable;

        private int _pressedButton = -1;    // the button the mouse went down on
        private MouseState _prevMouse;
        private Point _mousePos;

        // True from Show() until the player picks an option. While true, the rest of the game should ignore input.
        public bool IsVisible => _visible;

        public LevelCompletePopup(ViewportLayout layout, UiDraw ui)
        {
            _layout = layout;
            _ui = ui;
        }

        public void Show(string levelName, bool hasNextLevel, bool mainMenuAvailable)
        {
            _visible = true;
            _delay = ShowDelay;
            _waitForRelease = true;
            _pressedButton = -1;

            _levelName = levelName;
            _hasNextLevel = hasNextLevel;
            _mainMenuAvailable = mainMenuAvailable;
        }

        // Call every frame (even when hidden, so the mouse state stays current).
        // A button fires when the mouse is released on the same button it was pressed on.
        public PopupAction Update(MouseState mouse, float dt)
        {
            _mousePos = mouse.Position;

            bool pressed = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
            bool released = mouse.LeftButton == ButtonState.Released && _prevMouse.LeftButton == ButtonState.Pressed;
            _prevMouse = mouse;

            if (!_visible) return PopupAction.None;

            if (_delay > 0f)
            {
                _delay -= dt;
                return PopupAction.None;
            }

            if (_waitForRelease)
            {
                if (mouse.LeftButton == ButtonState.Released)
                    _waitForRelease = false;
                return PopupAction.None;
            }

            if (pressed)
                _pressedButton = ButtonAt(mouse.Position);

            if (released)
            {
                int pressedOn = _pressedButton;
                _pressedButton = -1;

                if (pressedOn >= 0 && ButtonAt(mouse.Position) == pressedOn)
                {
                    _visible = false;
                    return ActionFor(pressedOn);
                }
            }

            return PopupAction.None;
        }

        // ------------------------------------------------------------------
        //  Layout (all in window coordinates)
        // ------------------------------------------------------------------

        private Rectangle PanelRect
        {
            get
            {
                Viewport win = _layout.Window;
                int width = System.Math.Min(PanelMaxWidth, win.Width - 40);
                return new Rectangle((win.Width - width) / 2, (win.Height - PanelHeight) / 2, width, PanelHeight);
            }
        }

        private Rectangle ButtonRect(int index)
        {
            Rectangle panel = PanelRect;
            int width = (panel.Width - PanelPadding * 2 - ButtonGap * 2) / 3;

            return new Rectangle(
                panel.X + PanelPadding + index * (width + ButtonGap),
                panel.Bottom - PanelPadding - ButtonHeight,
                width,
                ButtonHeight);
        }

        private bool IsEnabled(int index)
        {
            if (index == 1) return _mainMenuAvailable;
            if (index == 2) return _hasNextLevel;
            return true;   // Restart is always available
        }

        // Which enabled button is under this point? -1 if none
        private int ButtonAt(Point point)
        {
            for (int i = 0; i < Labels.Length; i++)
            {
                if (IsEnabled(i) && ButtonRect(i).Contains(point))
                    return i;
            }
            return -1;
        }

        private static PopupAction ActionFor(int index)
        {
            if (index == 0) return PopupAction.Restart;
            if (index == 1) return PopupAction.MainMenu;
            return PopupAction.NextLevel;
        }

        // ------------------------------------------------------------------
        //  Drawing
        // ------------------------------------------------------------------

        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch sb)
        {
            if (!_visible || _delay > 0f) return;

            graphicsDevice.Viewport = _layout.Window;
            Viewport win = _layout.Window;

            sb.Begin(samplerState: SamplerState.PointClamp);

            // Dim everything behind the window
            _ui.Rect(sb, new Rectangle(0, 0, win.Width, win.Height), Color.Black * 0.6f);

            Rectangle panel = PanelRect;
            _ui.Rect(sb, panel, new Color(30, 30, 45));
            _ui.Border(sb, panel, 3, Color.White);

            // Title, drawn at double size and centred
            const string title = "Level Done!";
            const float titleScale = 2f;
            Vector2 titleSize = _ui.Font.MeasureString(title) * titleScale;
            sb.DrawString(
                _ui.Font,
                title,
                new Vector2(panel.Center.X - titleSize.X / 2f, panel.Y + 24),
                new Color(120, 220, 140),
                0f,
                Vector2.Zero,
                titleScale,
                SpriteEffects.None,
                0f);

            string subtitle = _hasNextLevel ? _levelName : "That was the last level!";
            _ui.FittedText(sb, subtitle, new Rectangle(panel.X + PanelPadding, panel.Y + 90, panel.Width - PanelPadding * 2, 30), Color.LightGray, true);

            for (int i = 0; i < Labels.Length; i++)
                DrawButton(sb, i);

            sb.End();
        }

        private void DrawButton(SpriteBatch sb, int index)
        {
            Rectangle rect = ButtonRect(index);
            bool enabled = IsEnabled(index);

            Color fill;
            Color border;
            Color text;

            if (!enabled)
            {
                fill = new Color(50, 50, 60);
                border = new Color(90, 90, 100);
                text = new Color(130, 130, 140);
            }
            else
            {
                fill = ButtonColors[index];
                border = Color.White;
                text = Color.White;

                if (rect.Contains(_mousePos))
                    fill = Color.Lerp(fill, Color.White, 0.2f);

                // Darken while the mouse is held down on this button
                if (_pressedButton == index && rect.Contains(_mousePos))
                    fill = Color.Lerp(fill, Color.Black, 0.3f);
            }

            _ui.Rect(sb, rect, fill);
            _ui.Border(sb, rect, 2, border);
            _ui.FittedText(sb, Labels[index], rect, text, true);
        }
    }
}