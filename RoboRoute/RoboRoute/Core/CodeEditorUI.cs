using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoboRoute.RoboRoute.Commands;
using System;
using System.Collections.Generic;

namespace RoboRoute.RoboRoute.Core
{
    // this file handles the middle panel (code queue + Run button) and the right panel (block palette),
    // including dragging blocks between them. It edits the ProgramRunner's queue;
    // it never moves the robot itself. it only passes commands/movements into the robot
    public class CodeEditorUI
    {
        private const int Margin = 10;
        private const int HeaderHeight = 40;
        private const int BlockHeight = 40;
        private const int BlockGap = 8;
        private const int SlotStep = BlockHeight + BlockGap;   // distance from one block's top to the next

        // Bottom of the middle panel: status line + Run button
        private const int ButtonHeight = 44;
        private const int StatusHeight = 24;
        private const int FooterHeight = ButtonHeight + StatusHeight + Margin * 2;

        // Queue scrolling
        private const int MaxQueueBlocks = 40;          // longest program the player can build
        private const int EdgeScrollZone = 30;          // px from the top/bottom of the queue where dragging auto-scrolls
        private const float EdgeScrollSpeed = 400f;     // px per second
        private const int ScrollbarWidth = 4;

        private readonly ViewportLayout _layout;
        private readonly ProgramRunner _runner;
        private readonly UiDraw _ui;

        // The blocks available in the right panel (unlimited supply of each)
        private readonly BlockType[] _palette =
        {
            BlockType.MoveForward,
            BlockType.TurnLeft,
            BlockType.TurnRight
        };

        private bool _dragging;
        private BlockType _dragType;
        private Point _dragGrabOffset;  // where inside the block the player grabbed it
        private Point _dragSize;

        private MouseState _prevMouse;
        private Point _mousePos;        // window coordinates

        private float _scrollY;         // how many pixels the queue is scrolled down (0 = top)
        private int _lastActiveStep = -1;

        private List<BlockType> Queue => _runner.Queue;

        public CodeEditorUI(ViewportLayout layout, ProgramRunner runner, UiDraw ui)
        {
            _layout = layout;
            _runner = runner;
            _ui = ui;
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
                HeaderHeight + index * SlotStep,
                _layout.Right.Width - Margin * 2,
                BlockHeight);
        }

        // Where a queue block is drawn right now (takes the scroll offset into account)
        private Rectangle QueueSlot(int index)
        {
            return new Rectangle(
                Margin,
                HeaderHeight + index * SlotStep - (int)_scrollY,
                _layout.Middle.Width - Margin * 2,
                BlockHeight);
        }

        // The Run/Stop button, pinned to the bottom of the middle panel
        private Rectangle RunButtonRect
        {
            get
            {
                return new Rectangle(
                    Margin,
                    _layout.Middle.Height - Margin - ButtonHeight,
                    _layout.Middle.Width - Margin * 2,
                    ButtonHeight);
            }
        }

        // The visible window onto the queue: between the header and the footer
        private int ListHeight => Math.Max(0, _layout.Middle.Height - HeaderHeight - FooterHeight);

        private Rectangle ListArea => new Rectangle(0, HeaderHeight, _layout.Middle.Width, ListHeight);

        // The queue plus one empty slot at the end to drop new blocks into
        private int ContentSlotCount => Math.Min(Queue.Count + 1, MaxQueueBlocks);

        private int ContentHeight => ContentSlotCount * SlotStep - BlockGap;

        private float MaxScroll => Math.Max(0, ContentHeight - ListHeight);

        private void ClampScroll()
        {
            _scrollY = MathHelper.Clamp(_scrollY, 0f, MaxScroll);
        }

        // Scrolls just enough to bring a queue block fully into view
        private void ScrollToShow(int index)
        {
            float top = index * SlotStep;
            float bottom = top + BlockHeight;

            if (top < _scrollY)
                _scrollY = top;
            else if (bottom > _scrollY + ListHeight)
                _scrollY = bottom - ListHeight;
        }

        // Is any part of this (local) rectangle inside the visible queue area?
        private bool IsInView(Rectangle slot)
        {
            return slot.Bottom > HeaderHeight && slot.Y < HeaderHeight + ListHeight;
        }

        // Which position in the queue a block dropped at this (local) point would take
        private int GetInsertIndex(Point local)
        {
            for (int i = 0; i < Queue.Count; i++)
            {
                if (local.Y < QueueSlot(i).Center.Y)
                    return i;
            }
            return Queue.Count;
        }

        // ------------------------------------------------------------------
        //  Input: scrolling + drag and drop + Run button clicks
        // ------------------------------------------------------------------

        // dt is optional so existing calls like Update(Mouse.GetState()) still work,
        // but pass the real frame time for smooth auto-scrolling: Update(Mouse.GetState(), dt)
        public void Update(MouseState mouse, float dt = 1f / 60f)
        {
            _mousePos = mouse.Position;

            bool pressed = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
            bool released = mouse.LeftButton == ButtonState.Released && _prevMouse.LeftButton == ButtonState.Pressed;

            // Mouse wheel scrolls the queue while the cursor is over the middle panel (one notch = one block)
            int wheel = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
            if (wheel != 0 && _layout.Middle.Bounds.Contains(mouse.Position))
                _scrollY -= wheel / 120f * SlotStep;

            if (pressed && !_dragging)
            {
                // Button first; otherwise try to pick up a block (not allowed while the program runs)
                if (!TryClickRunButton(mouse.Position) && !_runner.IsRunning)
                    TryStartDrag(mouse.Position);
            }

            if (released && _dragging)
                DropBlock(mouse.Position);

            // Dragging a block near the top or bottom of the queue scrolls it
            if (_dragging && _layout.Middle.Bounds.Contains(mouse.Position))
            {
                int localY = mouse.Y - _layout.Middle.Y;

                if (localY < HeaderHeight + EdgeScrollZone)
                    _scrollY -= EdgeScrollSpeed * dt;
                else if (localY > HeaderHeight + ListHeight - EdgeScrollZone)
                    _scrollY += EdgeScrollSpeed * dt;
            }

            // While the program runs, keep the executing block in view
            int active = _runner.IsRunning ? _runner.ActiveStep : -1;
            if (active >= 0 && active != _lastActiveStep)
                ScrollToShow(active);
            _lastActiveStep = active;

            ClampScroll();   // the queue may have grown, shrunk, or the window may have been resized

            _prevMouse = mouse;
        }

        private bool TryClickRunButton(Point pos)
        {
            if (!_layout.Middle.Bounds.Contains(pos)) return false;
            if (!RunButtonRect.Contains(ToLocal(pos, _layout.Middle))) return false;

            if (_runner.IsRunning)
                _runner.Stop();
            else
                _runner.Start();   // does nothing if the queue is empty

            return true;
        }

        private void TryStartDrag(Point pos)
        {
            if (_layout.Right.Bounds.Contains(pos))
            {
                // Grab a copy from the palette
                Point local = ToLocal(pos, _layout.Right);
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
            else if (_layout.Middle.Bounds.Contains(pos))
            {
                Point local = ToLocal(pos, _layout.Middle);

                // Blocks scrolled under the header or footer can't be grabbed
                if (!ListArea.Contains(local)) return;

                // Pick a block back up out of the queue (to reorder it or throw it away)
                for (int i = 0; i < Queue.Count; i++)
                {
                    Rectangle slot = QueueSlot(i);
                    if (slot.Contains(local))
                    {
                        BlockType type = Queue[i];
                        Queue.RemoveAt(i);
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
            _runner.ClearStatus();
        }

        private void DropBlock(Point pos)
        {
            _dragging = false;

            if (_layout.Middle.Bounds.Contains(pos) && Queue.Count < MaxQueueBlocks)
            {
                int index = GetInsertIndex(ToLocal(pos, _layout.Middle));
                Queue.Insert(index, _dragType);
            }
            // Dropped anywhere else (or the queue is full): the block is discarded
        }

        // ------------------------------------------------------------------
        //  Drawing
        // ------------------------------------------------------------------

        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch sb)
        {
            // MIDDLE viewport (code queue): (0,0) is its own top-left corner
            graphicsDevice.Viewport = _layout.Middle;
            sb.Begin(samplerState: SamplerState.PointClamp);
            DrawMiddlePanel(sb);
            sb.End();

            // RIGHT viewport (block palette)
            graphicsDevice.Viewport = _layout.Right;
            sb.Begin(samplerState: SamplerState.PointClamp);
            DrawRightPanel(sb);
            sb.End();

            // The block being dragged goes LAST, on a full-window viewport.
            // If it were drawn inside a panel's viewport it would be clipped at that panel's edge.
            if (_dragging)
            {
                graphicsDevice.Viewport = _layout.Window;

                Rectangle ghost = new Rectangle(
                    _mousePos.X - _dragGrabOffset.X,
                    _mousePos.Y - _dragGrabOffset.Y,
                    _dragSize.X, _dragSize.Y);

                sb.Begin(samplerState: SamplerState.PointClamp);
                _ui.Block(sb, ghost, _dragType, 0.85f);
                sb.End();
            }
        }

        private void DrawMiddlePanel(SpriteBatch sb)
        {
            Viewport mid = _layout.Middle;
            Color panelColor = new Color(30, 30, 40);

            // Panel background
            _ui.Rect(sb, new Rectangle(0, 0, mid.Width, mid.Height), panelColor);

            // ---- Scrolling content: drawn first, then the header and footer are painted over it ----

            // Faint outlines for the empty slots (enough to fill the view, never more than the queue limit)
            int fitting = (ListHeight + BlockGap) / SlotStep;
            int outlineCount = Math.Min(MaxQueueBlocks, Math.Max(ContentSlotCount, fitting));
            for (int i = 0; i < outlineCount; i++)
            {
                Rectangle slot = QueueSlot(i);
                if (IsInView(slot))
                    _ui.Border(sb, slot, 1, new Color(60, 60, 75));
            }

            // The blocks in the queue (the one being executed gets a yellow outline)
            for (int i = 0; i < Queue.Count; i++)
            {
                Rectangle slot = QueueSlot(i);
                if (!IsInView(slot)) continue;

                _ui.Block(sb, slot, Queue[i]);

                if (_runner.IsRunning && i == _runner.ActiveStep)
                    _ui.Border(sb, slot, 4, Color.Yellow);
            }

            // Yellow line showing where the dragged block will land
            if (_dragging && mid.Bounds.Contains(_mousePos) && Queue.Count < MaxQueueBlocks)
            {
                int index = GetInsertIndex(ToLocal(_mousePos, mid));
                Rectangle slot = QueueSlot(index);
                _ui.Rect(sb, new Rectangle(Margin, slot.Y - BlockGap / 2 - 1, slot.Width, 3), Color.Yellow);
            }

            // ---- Header and footer hide any block that has scrolled underneath them ----
            _ui.Rect(sb, new Rectangle(0, 0, mid.Width, HeaderHeight), panelColor);
            _ui.Rect(sb, new Rectangle(0, mid.Height - FooterHeight, mid.Width, FooterHeight), panelColor);

            // Divider line on the left edge
            _ui.Rect(sb, new Rectangle(0, 0, 2, mid.Height), Color.White);

            _ui.Text(sb, "Code Queue", new Vector2(Margin, 10), Color.White);

            DrawScrollbar(sb);

            // Footer: status line, then the Run/Stop button
            if (_runner.Status.Length > 0)
            {
                Rectangle statusArea = new Rectangle(
                    Margin,
                    RunButtonRect.Y - StatusHeight,
                    mid.Width - Margin * 2,
                    StatusHeight);
                _ui.FittedText(sb, _runner.Status, statusArea, _runner.StatusColor, false);
            }

            DrawRunButton(sb);
        }

        // A thin bar in the right margin; only shown when the queue is longer than the view
        private void DrawScrollbar(SpriteBatch sb)
        {
            if (MaxScroll <= 0 || ListHeight <= 0) return;

            int x = _layout.Middle.Width - Margin / 2 - ScrollbarWidth / 2 - 1;

            Rectangle track = new Rectangle(x, HeaderHeight, ScrollbarWidth, ListHeight);
            _ui.Rect(sb, track, new Color(50, 50, 65));

            int thumbHeight = Math.Max(20, (int)((float)ListHeight * ListHeight / ContentHeight));
            int thumbY = HeaderHeight + (int)((ListHeight - thumbHeight) * (_scrollY / MaxScroll));
            _ui.Rect(sb, new Rectangle(x, thumbY, ScrollbarWidth, thumbHeight), new Color(140, 140, 160));
        }

        private void DrawRunButton(SpriteBatch sb)
        {
            Viewport mid = _layout.Middle;
            Rectangle r = RunButtonRect;

            bool hover = !_dragging
                         && mid.Bounds.Contains(_mousePos)
                         && r.Contains(ToLocal(_mousePos, mid));

            string label;
            Color color;

            if (_runner.IsRunning)
            {
                label = "STOP";
                color = new Color(170, 55, 55);
            }
            else if (Queue.Count == 0)
            {
                label = "RUN";
                color = new Color(70, 70, 80);   // greyed out: nothing to run
                hover = false;
            }
            else
            {
                label = "RUN";
                color = new Color(46, 140, 80);
            }

            if (hover)
                color = Color.Lerp(color, Color.White, 0.2f);

            _ui.Rect(sb, r, color);
            _ui.Border(sb, r, 2, Color.White);
            _ui.FittedText(sb, label, r, Color.White, true);
        }

        private void DrawRightPanel(SpriteBatch sb)
        {
            Viewport right = _layout.Right;

            // Panel background (slightly different shade so you can tell them apart) + divider line
            _ui.Rect(sb, new Rectangle(0, 0, right.Width, right.Height), new Color(45, 35, 55));
            _ui.Rect(sb, new Rectangle(0, 0, 2, right.Height), Color.White);

            _ui.Text(sb, "Blocks", new Vector2(Margin, 10), Color.White);

            for (int i = 0; i < _palette.Length; i++)
                _ui.Block(sb, PaletteSlot(i), _palette[i]);
        }
    }
}