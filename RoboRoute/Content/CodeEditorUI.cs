using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoboRoute.RoboRoute.Commands;
using System;
using System.Collections.Generic;

namespace RoboRoute.RoboRoute.Content
{
    // The middle panel (code queue + Run button) and the right panel (block palette),
    // including dragging blocks between them. It edits the ProgramRunner's queue;
    // it never moves the robot itself.
    public class CodeEditorUI
    {
        private const int Margin = 10;
        private const int HeaderHeight = 40;
        private const int BlockHeight = 40;
        private const int BlockGap = 8;

        // Bottom of the middle panel: status line + Run button
        private const int ButtonHeight = 44;
        private const int StatusHeight = 24;
        private const int FooterHeight = ButtonHeight + StatusHeight + Margin * 2;

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
                HeaderHeight + index * (BlockHeight + BlockGap),
                _layout.Right.Width - Margin * 2,
                BlockHeight);
        }

        private Rectangle QueueSlot(int index)
        {
            return new Rectangle(
                Margin,
                HeaderHeight + index * (BlockHeight + BlockGap),
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

        // How many blocks fit in the middle panel (above the footer)
        private int QueueCapacity
        {
            get
            {
                int available = _layout.Middle.Height - HeaderHeight - FooterHeight;
                return Math.Max(0, (available + BlockGap) / (BlockHeight + BlockGap));
            }
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
        //  Input: drag and drop + Run button clicks
        // ------------------------------------------------------------------

        public void Update(MouseState mouse)
        {
            _mousePos = mouse.Position;

            bool pressed = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
            bool released = mouse.LeftButton == ButtonState.Released && _prevMouse.LeftButton == ButtonState.Pressed;

            if (pressed && !_dragging)
            {
                // Button first; otherwise try to pick up a block (not allowed while the program runs)
                if (!TryClickRunButton(mouse.Position) && !_runner.IsRunning)
                    TryStartDrag(mouse.Position);
            }

            if (released && _dragging)
                DropBlock(mouse.Position);

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
                // Pick a block back up out of the queue (to reorder it or throw it away)
                Point local = ToLocal(pos, _layout.Middle);
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

            if (_layout.Middle.Bounds.Contains(pos) && Queue.Count < QueueCapacity)
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

            // Panel background + divider line on the left edge
            _ui.Rect(sb, new Rectangle(0, 0, mid.Width, mid.Height), new Color(30, 30, 40));
            _ui.Rect(sb, new Rectangle(0, 0, 2, mid.Height), Color.White);

            _ui.Text(sb, "Code Queue", new Vector2(Margin, 10), Color.White);

            // Faint outlines showing how many blocks fit
            for (int i = 0; i < QueueCapacity; i++)
                _ui.Border(sb, QueueSlot(i), 1, new Color(60, 60, 75));

            // The blocks in the queue (the one being executed gets a yellow outline)
            for (int i = 0; i < Queue.Count; i++)
            {
                _ui.Block(sb, QueueSlot(i), Queue[i]);

                if (_runner.IsRunning && i == _runner.ActiveStep)
                    _ui.Border(sb, QueueSlot(i), 4, Color.Yellow);
            }

            // Yellow line showing where the dragged block will land
            if (_dragging && mid.Bounds.Contains(_mousePos) && Queue.Count < QueueCapacity)
            {
                int index = GetInsertIndex(ToLocal(_mousePos, mid));
                Rectangle slot = QueueSlot(index);
                _ui.Rect(sb, new Rectangle(Margin, slot.Y - BlockGap / 2 - 1, slot.Width, 3), Color.Yellow);
            }

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