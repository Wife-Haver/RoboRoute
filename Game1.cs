using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoboRoute.RoboRoute.Commands;
using RoboRoute.RoboRoute.Core;
using System;
using System.Collections.Generic;



namespace RoboRoute
{
    public class Game1 : Game

    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private LevelTileMap _LevelTileMap;

        private Robot robot;
        private const int VirtualWidth = 320;   // game's internal resolution
        private const int VirtualHeight = 180;  // (16:9, fits ~13x7 tiles of 24px)
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
        private SpriteFont _font; // add a SpriteFont called "Font" in Content.mgcb

        // ---- Code blocks / drag and drop ----
        private const int Margin = 10;
        private const int HeaderHeight = 40;
        private const int BlockHeight = 40;
        private const int BlockGap = 8;

        // The blocks available in the right panel (unlimited supply of each)
        private readonly BlockType[] _palette =
        {
            BlockType.MoveForward,
            BlockType.TurnLeft,
            BlockType.TurnRight
        };

        // The player's program, top to bottom (middle panel)
        private readonly List<BlockType> _queue = new List<BlockType>();

        private bool _dragging;
        private BlockType _dragType;
        private Point _dragGrabOffset;  // where inside the block the player grabbed it
        private Point _dragSize;

        private MouseState _prevMouse;
        private Point _mousePos;        // window coordinates

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

            _font = Content.Load<SpriteFont>("Font");

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

            // Mouse + drag and drop
            var mouse = Mouse.GetState();
            _mousePos = mouse.Position;
            UpdateDragAndDrop(mouse);
            _prevMouse = mouse;

            robot.Update(gameTime, kb, _LevelTileMap.IsWalkable);

            base.Update(gameTime);
        }

        // ------------------------------------------------------------------
        //  Layout helpers (all rectangles are in the panel's LOCAL coordinates)
        // ------------------------------------------------------------------

        private static Point ToLocal(Point windowPos, Viewport vp)
        {
            return new Point(windowPos.X - vp.X, windowPos.Y - vp.Y);
        }

        private Rectangle PaletteSlot(int index)
        {
            return new Rectangle(
                Margin,
                HeaderHeight + index * (BlockHeight + BlockGap),
                _rightViewport.Width - Margin * 2,
                BlockHeight);
        }

        private Rectangle QueueSlot(int index)
        {
            return new Rectangle(
                Margin,
                HeaderHeight + index * (BlockHeight + BlockGap),
                _middleViewport.Width - Margin * 2,
                BlockHeight);
        }

        // How many blocks fit in the middle panel
        private int QueueCapacity
        {
            get
            {
                int available = _middleViewport.Height - HeaderHeight - Margin;
                return Math.Max(0, (available + BlockGap) / (BlockHeight + BlockGap));
            }
        }

        // Which position in the queue a block dropped at this (local) point would take
        private int GetInsertIndex(Point local)
        {
            for (int i = 0; i < _queue.Count; i++)
            {
                if (local.Y < QueueSlot(i).Center.Y)
                    return i;
            }
            return _queue.Count;
        }

        // ------------------------------------------------------------------
        //  Drag and drop
        // ------------------------------------------------------------------

        private void UpdateDragAndDrop(MouseState mouse)
        {
            bool pressed = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
            bool released = mouse.LeftButton == ButtonState.Released && _prevMouse.LeftButton == ButtonState.Pressed;

            if (pressed && !_dragging)
                TryStartDrag(mouse.Position);

            if (released && _dragging)
                DropBlock(mouse.Position);
        }

        private void TryStartDrag(Point pos)
        {
            if (_rightViewport.Bounds.Contains(pos))
            {
                // Grab a copy from the palette
                Point local = ToLocal(pos, _rightViewport);
                for (int i = 0; i < _palette.Length; i++)
                {
                    Rectangle slot = PaletteSlot(i);
                    if (slot.Contains(local))
                    {
                        BeginDrag(_palette[i], local, slot);
                        return;
                    }
                }
            }
            else if (_middleViewport.Bounds.Contains(pos))
            {
                // Pick a block back up out of the queue (to reorder it or throw it away)
                Point local = ToLocal(pos, _middleViewport);
                for (int i = 0; i < _queue.Count; i++)
                {
                    Rectangle slot = QueueSlot(i);
                    if (slot.Contains(local))
                    {
                        BlockType type = _queue[i];
                        _queue.RemoveAt(i);
                        BeginDrag(type, local, slot);
                        return;
                    }
                }
            }
        }

        private void BeginDrag(BlockType type, Point localMouse, Rectangle slot)
        {
            _dragging = true;
            _dragType = type;
            _dragGrabOffset = new Point(localMouse.X - slot.X, localMouse.Y - slot.Y);
            _dragSize = new Point(slot.Width, slot.Height);
        }

        private void DropBlock(Point pos)
        {
            _dragging = false;

            if (_middleViewport.Bounds.Contains(pos) && _queue.Count < QueueCapacity)
            {
                int index = GetInsertIndex(ToLocal(pos, _middleViewport));
                _queue.Insert(index, _dragType);
            }
            // Dropped anywhere else (or the queue is full): the block is discarded
        }

        // ------------------------------------------------------------------
        //  Drawing
        // ------------------------------------------------------------------

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

            // 4. MIDDLE viewport (code queue): (0,0) is its own top-left corner
            GraphicsDevice.Viewport = _middleViewport;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawMiddlePanel(_spriteBatch);
            _spriteBatch.End();

            // 5. RIGHT viewport (block palette)
            GraphicsDevice.Viewport = _rightViewport;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            DrawRightPanel(_spriteBatch);
            _spriteBatch.End();

            // 6. The block being dragged goes LAST, on a full-window viewport.
            //    If it were drawn inside a panel's viewport it would be clipped at that panel's edge.
            if (_dragging)
            {
                GraphicsDevice.Viewport = new Viewport(0, 0, winW, winH);

                Rectangle ghost = new Rectangle(
                    _mousePos.X - _dragGrabOffset.X,
                    _mousePos.Y - _dragGrabOffset.Y,
                    _dragSize.X, _dragSize.Y);

                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                DrawBlock(_spriteBatch, ghost, _dragType, 0.85f);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }

        private void DrawMiddlePanel(SpriteBatch sb)
        {
            // Panel background
            sb.Draw(_pixel, new Rectangle(0, 0, _middleViewport.Width, _middleViewport.Height), new Color(30, 30, 40));

            // Divider line on the left edge
            sb.Draw(_pixel, new Rectangle(0, 0, 2, _middleViewport.Height), Color.White);

            sb.DrawString(_font, "Code Queue", new Vector2(Margin, 10), Color.White);

            // Faint outlines showing how many blocks fit
            for (int i = 0; i < QueueCapacity; i++)
                DrawBorder(sb, QueueSlot(i), 1, new Color(60, 60, 75));

            // The blocks in the queue
            for (int i = 0; i < _queue.Count; i++)
                DrawBlock(sb, QueueSlot(i), _queue[i]);

            // Yellow line showing where the dragged block will land
            if (_dragging && _middleViewport.Bounds.Contains(_mousePos) && _queue.Count < QueueCapacity)
            {
                int index = GetInsertIndex(ToLocal(_mousePos, _middleViewport));
                Rectangle slot = QueueSlot(index);
                sb.Draw(_pixel, new Rectangle(Margin, slot.Y - BlockGap / 2 - 1, slot.Width, 3), Color.Yellow);
            }
        }

        private void DrawRightPanel(SpriteBatch sb)
        {
            // Panel background (slightly different shade so you can tell them apart)
            sb.Draw(_pixel, new Rectangle(0, 0, _rightViewport.Width, _rightViewport.Height), new Color(45, 35, 55));

            // Divider line on the left edge
            sb.Draw(_pixel, new Rectangle(0, 0, 2, _rightViewport.Height), Color.White);

            sb.DrawString(_font, "Blocks", new Vector2(Margin, 10), Color.White);

            for (int i = 0; i < _palette.Length; i++)
                DrawBlock(sb, PaletteSlot(i), _palette[i]);
        }

        // Draws one code block: coloured box, border, and a centred label that shrinks to fit
        private void DrawBlock(SpriteBatch sb, Rectangle r, BlockType type, float alpha = 1f)
        {
            sb.Draw(_pixel, r, BlockInfo.GetColor(type) * alpha);
            DrawBorder(sb, r, 2, Color.White * alpha);

            string label = BlockInfo.GetLabel(type);
            Vector2 size = _font.MeasureString(label);
            float textScale = Math.Min(1f, (r.Width - 12) / size.X);

            Vector2 pos = new Vector2(
                r.X + (r.Width - size.X * textScale) / 2f,
                r.Y + (r.Height - size.Y * textScale) / 2f);

            sb.DrawString(_font, label, pos, Color.White * alpha, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
        }

        private void DrawBorder(SpriteBatch sb, Rectangle r, int thickness, Color color)
        {
            sb.Draw(_pixel, new Rectangle(r.X, r.Y, r.Width, thickness), color);                    // top
            sb.Draw(_pixel, new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness), color);   // bottom
            sb.Draw(_pixel, new Rectangle(r.X, r.Y, thickness, r.Height), color);                   // left
            sb.Draw(_pixel, new Rectangle(r.Right - thickness, r.Y, thickness, r.Height), color);   // right
        }
    }
}