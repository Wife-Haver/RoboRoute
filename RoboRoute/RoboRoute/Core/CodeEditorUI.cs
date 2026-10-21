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
    //
    // The level decides what the player gets: which blocks appear in the palette, how many of each
    // (shown as a small number on the block), and the total number of blocks allowed in the queue.
    //
    // The queue is a tree: a Repeat block holds child blocks. To draw and hit-test it, the tree is
    // flattened every frame into "rows" (see RebuildRows): one row per block, plus an "end" bar row
    // after each Repeat block's children.
    public class CodeEditorUI
    {
        private const int Margin = 10;
        private const int HeaderHeight = 40;
        private const int BlockHeight = 40;
        private const int BlockGap = 8;
        private const int SlotStep = BlockHeight + BlockGap;   // distance from one block's top to the next

        // Loops
        private const int LoopEndHeight = 20;      // the bar that closes a Repeat block
        private const int Indent = 16;             // how far children are pushed right per nesting level
        private const int MaxLoopNesting = 3;      // loops inside loops inside loops, at most
        private const int CountBoxWidth = 44;

        // The small number on each palette block
        private const int BadgeSize = 28;

        // Bottom of the middle panel: status line + Run button
        private const int ButtonHeight = 44;
        private const int StatusHeight = 24;
        private const int FooterHeight = ButtonHeight + StatusHeight + Margin * 2;

        // Queue scrolling
        private const int EdgeScrollZone = 30;          // px from the top/bottom of the queue where dragging auto-scrolls
        private const float EdgeScrollSpeed = 400f;     // px per second
        private const int ScrollbarWidth = 4;

        private enum RowKind { Block, LoopEnd }

        // One line of the flattened queue
        private struct Row
        {
            public RowKind Kind;
            public Block Block;          // the block this row shows (for LoopEnd: the loop it closes)
            public List<Block> List;     // the list that contains Block
            public int Index;            // Block's position inside List
            public int Depth;            // how many loops enclose it (0 = top level)
            public int ContentY;         // top of the row in scrolling-content coordinates
            public int Height;
            public int EndIndex;         // loop headers only: index of the matching LoopEnd row
        }

        // Where a dragged block would be inserted
        private struct DropTarget
        {
            public List<Block> List;
            public int Index;
            public int Depth;            // how many loops enclose the drop point
            public int LineY;            // content Y where the insert line is drawn
        }

        private readonly ViewportLayout _layout;
        private readonly ProgramRunner _runner;
        private readonly UiDraw _ui;

        private LevelData _level;

        // Every kind of block that exists, in the order they appear in the palette
        private readonly BlockType[] _allBlockTypes =
        {
            BlockType.MoveForward,
            BlockType.TurnLeft,
            BlockType.TurnRight,
            BlockType.Repeat
        };

        // The blocks shown in the right panel: only the ones this level gives the player (count > 0)
        private readonly List<BlockType> _palette = new List<BlockType>();

        private readonly List<Row> _rows = new List<Row>();
        private int _contentCursor;     // content Y just below the last row

        private bool _dragging;
        private Block _dragBlock;       // the block being dragged (a loop brings its children along)
        private Point _dragGrabOffset;  // where inside the block the player grabbed it
        private Point _dragSize;

        // Typing a number into a Repeat block
        private Block _editBlock;       // the loop being edited, null = not editing
        private string _editText = "";
        private bool _editFresh;        // true until the first key press, which replaces the old number

        private MouseState _prevMouse;
        private KeyboardState _prevKeyboard;
        private Point _mousePos;        // window coordinates

        private float _scrollY;         // how many pixels the queue is scrolled down (0 = top)
        private Block _lastActiveBlock;

        private List<Block> Queue => _runner.Queue;

        // Most blocks the player may place in total (loop contents count too)
        private int MaxBlocks => _level.MaxBlocksAllowed;

        public CodeEditorUI(ViewportLayout layout, ProgramRunner runner, UiDraw ui, LevelData level)
        {
            _layout = layout;
            _runner = runner;
            _ui = ui;

            SetLevel(level);
        }

        // Switches to a level: rebuilds the palette from its given blocks and empties the queue.
        // Call this again when the player moves on to the next level.
        public void SetLevel(LevelData level)
        {
            _level = level;

            _editBlock = null;
            _dragging = false;
            _dragBlock = null;

            _palette.Clear();
            foreach (BlockType type in _allBlockTypes)
            {
                if (level.GetGivenCount(type) > 0)
                    _palette.Add(type);
            }

            Queue.Clear();
            _runner.ClearStatus();
            _scrollY = 0f;
            RebuildRows();
        }

        // ------------------------------------------------------------------
        //  Block limits
        // ------------------------------------------------------------------

        private static int CountOfType(Block block, BlockType type)
        {
            int count = block.Type == type ? 1 : 0;
            foreach (Block child in block.Children)
                count += CountOfType(child, type);
            return count;
        }

        // How many blocks of this type are in the queue right now (including inside loops)
        private int UsedCount(BlockType type)
        {
            int used = 0;
            foreach (Block block in Queue)
                used += CountOfType(block, type);
            return used;
        }

        // How many more of this type the player may still place: the number shown on the palette block
        private int RemainingOf(BlockType type)
        {
            return Math.Max(0, _level.GetGivenCount(type) - UsedCount(type));
        }

        private int TotalBlocks()
        {
            int total = 0;
            foreach (Block block in Queue)
                total += block.TotalBlocks();
            return total;
        }

        // ------------------------------------------------------------------
        //  Rows: the queue tree flattened into lines
        // ------------------------------------------------------------------

        private void RebuildRows()
        {
            _rows.Clear();
            _contentCursor = 0;
            AddRows(Queue, 0);
        }

        private void AddRows(List<Block> list, int depth)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Block block = list[i];

                int headerIndex = _rows.Count;
                _rows.Add(new Row
                {
                    Kind = RowKind.Block,
                    Block = block,
                    List = list,
                    Index = i,
                    Depth = depth,
                    ContentY = _contentCursor,
                    Height = BlockHeight
                });
                _contentCursor += BlockHeight + BlockGap;

                if (block.IsLoop)
                {
                    AddRows(block.Children, depth + 1);

                    // Remember where this loop's closing bar is (used to draw the side spine)
                    Row header = _rows[headerIndex];
                    header.EndIndex = _rows.Count;
                    _rows[headerIndex] = header;

                    _rows.Add(new Row
                    {
                        Kind = RowKind.LoopEnd,
                        Block = block,
                        List = list,
                        Index = i,
                        Depth = depth,
                        ContentY = _contentCursor,
                        Height = LoopEndHeight
                    });
                    _contentCursor += LoopEndHeight + BlockGap;
                }
            }
        }

        private int FindRowIndex(Block block)
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].Kind == RowKind.Block && _rows[i].Block == block)
                    return i;
            }
            return -1;
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

        // Where a queue row is drawn right now (takes nesting and the scroll offset into account)
        private Rectangle RowRect(Row row)
        {
            int indent = row.Depth * Indent;
            return new Rectangle(
                Margin + indent,
                HeaderHeight + row.ContentY - (int)_scrollY,
                _layout.Middle.Width - Margin * 2 - indent,
                row.Height);
        }

        // The little box on a Repeat block where the number is typed
        private Rectangle CountBoxRect(Rectangle blockRect)
        {
            int labelWidth = (int)_ui.Font.MeasureString("Repeat").X;
            return new Rectangle(
                blockRect.X + 8 + labelWidth + 8,
                blockRect.Y + 7,
                CountBoxWidth,
                blockRect.Height - 14);
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

        // All the rows plus one empty slot at the end to drop new blocks into
        private int ContentHeight => _contentCursor + BlockHeight;

        private float MaxScroll => Math.Max(0, ContentHeight - ListHeight);

        private void ClampScroll()
        {
            _scrollY = MathHelper.Clamp(_scrollY, 0f, MaxScroll);
        }

        // Scrolls just enough to bring a row fully into view
        private void ScrollToShow(int rowIndex)
        {
            Row row = _rows[rowIndex];
            float top = row.ContentY;
            float bottom = top + row.Height;

            if (top < _scrollY)
                _scrollY = top;
            else if (bottom > _scrollY + ListHeight)
                _scrollY = bottom - ListHeight;
        }

        // Is any part of this (local) rectangle inside the visible queue area?
        private bool IsInView(Rectangle rect)
        {
            return rect.Bottom > HeaderHeight && rect.Y < HeaderHeight + ListHeight;
        }

        // ------------------------------------------------------------------
        //  Where would a dropped block go?
        // ------------------------------------------------------------------

        // Rule: the block goes BEFORE the first row whose middle is below the cursor.
        //  - before a normal block or a Repeat header: into that block's own list, just above it
        //  - before a loop's closing bar: at the end of that loop's children
        //  - below everything: at the end of the top-level queue
        // So the top half of a closing bar means "inside the loop" and the bottom half means "after the loop".
        private DropTarget GetDropTarget(Point local)
        {
            foreach (Row row in _rows)
            {
                if (local.Y >= RowRect(row).Center.Y) continue;

                if (row.Kind == RowKind.LoopEnd)
                {
                    return new DropTarget
                    {
                        List = row.Block.Children,
                        Index = row.Block.Children.Count,
                        Depth = row.Depth + 1,
                        LineY = row.ContentY
                    };
                }

                return new DropTarget
                {
                    List = row.List,
                    Index = row.Index,
                    Depth = row.Depth,
                    LineY = row.ContentY
                };
            }

            return new DropTarget { List = Queue, Index = Queue.Count, Depth = 0, LineY = _contentCursor };
        }

        // Is the dragged block (and anything inside it) allowed to go here?
        // The queue doesn't contain the dragged block while it's in the player's hand,
        // so these checks are "what's already placed" plus "what I'm holding".
        private bool CanDrop(DropTarget target)
        {
            if (_dragBlock == null) return false;

            // Total limit for the level
            if (TotalBlocks() + _dragBlock.TotalBlocks() > MaxBlocks) return false;

            // Not nested too deeply
            if (target.Depth + _dragBlock.LoopDepth() > MaxLoopNesting) return false;

            // Per-type limits (the "given" counts)
            foreach (BlockType type in _allBlockTypes)
            {
                if (UsedCount(type) + CountOfType(_dragBlock, type) > _level.GetGivenCount(type))
                    return false;
            }

            return true;
        }

        // ------------------------------------------------------------------
        //  Input: typing + scrolling + drag and drop + Run button clicks
        // ------------------------------------------------------------------

        // dt is optional so existing calls like Update(Mouse.GetState()) still work,
        // but pass the real frame time for smooth auto-scrolling: Update(Mouse.GetState(), dt)
        public void Update(MouseState mouse, float dt = 1f / 60f)
        {
            KeyboardState kb = Keyboard.GetState();
            _mousePos = mouse.Position;

            bool pressed = mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;
            bool released = mouse.LeftButton == ButtonState.Released && _prevMouse.LeftButton == ButtonState.Pressed;

            RebuildRows();

            if (_editBlock != null)
                UpdateTyping(kb);

            // Mouse wheel scrolls the queue while the cursor is over the middle panel (one notch = one block)
            int wheel = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
            if (wheel != 0 && _layout.Middle.Bounds.Contains(mouse.Position))
                _scrollY -= wheel / 120f * SlotStep;

            if (pressed && !_dragging)
            {
                // Clicking anywhere except the number box being edited finishes the edit
                if (_editBlock != null && !IsOverCountBox(mouse.Position, _editBlock))
                    CommitEdit();

                // Button first; otherwise number boxes, then picking up a block (none of it while the program runs)
                if (!TryClickRunButton(mouse.Position) && !_runner.IsRunning)
                {
                    if (!TryClickCountBox(mouse.Position))
                        TryStartDrag(mouse.Position);
                }
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
            Block active = _runner.IsRunning ? _runner.ActiveBlock : null;
            if (active != null && active != _lastActiveBlock)
            {
                int rowIndex = FindRowIndex(active);
                if (rowIndex >= 0)
                    ScrollToShow(rowIndex);
            }
            _lastActiveBlock = active;

            ClampScroll();   // the queue may have grown, shrunk, or the window may have been resized

            _prevMouse = mouse;
            _prevKeyboard = kb;
        }

        // ---- Typing a loop count ----

        private bool KeyPressed(KeyboardState kb, Keys key)
        {
            return kb.IsKeyDown(key) && _prevKeyboard.IsKeyUp(key);
        }

        private void UpdateTyping(KeyboardState kb)
        {
            for (int d = 0; d <= 9; d++)
            {
                if (KeyPressed(kb, Keys.D0 + d) || KeyPressed(kb, Keys.NumPad0 + d))
                {
                    if (_editFresh)
                    {
                        _editText = "";
                        _editFresh = false;
                    }

                    if (_editText.Length < 2)       // counts go up to 99
                        _editText += d.ToString();
                }
            }

            if (KeyPressed(kb, Keys.Back))
            {
                if (_editFresh)
                {
                    _editText = "";
                    _editFresh = false;
                }
                else if (_editText.Length > 0)
                {
                    _editText = _editText.Substring(0, _editText.Length - 1);
                }
            }

            if (KeyPressed(kb, Keys.Enter))
                CommitEdit();
        }

        private void StartEdit(Block loop)
        {
            CommitEdit();
            _editBlock = loop;
            _editText = loop.Count.ToString();
            _editFresh = true;
        }

        // Applies the typed number (kept between 1 and 99; ignored if the box was left empty)
        private void CommitEdit()
        {
            if (_editBlock == null) return;

            int typed;
            if (int.TryParse(_editText, out typed))
                _editBlock.Count = MathHelper.Clamp(typed, Block.MinCount, Block.MaxCount);

            _editBlock = null;
        }

        private bool IsOverCountBox(Point windowPos, Block loop)
        {
            if (!_layout.Middle.Bounds.Contains(windowPos)) return false;

            Point local = ToLocal(windowPos, _layout.Middle);
            if (!ListArea.Contains(local)) return false;

            int rowIndex = FindRowIndex(loop);
            if (rowIndex < 0) return false;

            return CountBoxRect(RowRect(_rows[rowIndex])).Contains(local);
        }

        private bool TryClickCountBox(Point pos)
        {
            if (!_layout.Middle.Bounds.Contains(pos)) return false;

            Point local = ToLocal(pos, _layout.Middle);
            if (!ListArea.Contains(local)) return false;

            foreach (Row row in _rows)
            {
                if (row.Kind != RowKind.Block || !row.Block.IsLoop) continue;

                if (CountBoxRect(RowRect(row)).Contains(local))
                {
                    if (_editBlock != row.Block)
                        StartEdit(row.Block);
                    return true;
                }
            }
            return false;
        }

        // ---- Buttons and dragging ----

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
                // Grab a fresh block from the palette (only if the level still gives you one)
                Point local = ToLocal(pos, _layout.Right);
                for (int i = 0; i < _palette.Count; i++)
                {
                    Rectangle slot = PaletteSlot(i);
                    if (!slot.Contains(local)) continue;

                    if (RemainingOf(_palette[i]) <= 0) return;   // none left

                    BeginDrag(new Block(_palette[i]), local, slot);
                    return;
                }
            }
            else if (_layout.Middle.Bounds.Contains(pos))
            {
                Point local = ToLocal(pos, _layout.Middle);

                // Blocks scrolled under the header or footer can't be grabbed
                if (!ListArea.Contains(local)) return;

                // Pick a block back up out of the queue (to reorder it or throw it away).
                // Picking up a Repeat block takes everything inside it too.
                foreach (Row row in _rows)
                {
                    if (row.Kind != RowKind.Block) continue;

                    Rectangle slot = RowRect(row);
                    if (slot.Contains(local))
                    {
                        row.List.RemoveAt(row.Index);
                        RebuildRows();
                        BeginDrag(row.Block, local, slot);
                        return;
                    }
                }
            }
        }

        private void BeginDrag(Block block, Point localMouse, Rectangle slot)
        {
            _dragging = true;
            _dragBlock = block;
            _dragGrabOffset = new Point(localMouse.X - slot.X, localMouse.Y - slot.Y);
            _dragSize = new Point(slot.Width, BlockHeight);
            _runner.ClearStatus();
        }

        private void DropBlock(Point pos)
        {
            _dragging = false;

            if (_layout.Middle.Bounds.Contains(pos))
            {
                DropTarget target = GetDropTarget(ToLocal(pos, _layout.Middle));

                if (CanDrop(target))
                {
                    target.List.Insert(target.Index, _dragBlock);
                    RebuildRows();
                }
            }
            // Dropped anywhere else (or somewhere not allowed): the block is discarded,
            // which also gives its count (and its contents') back to the palette

            _dragBlock = null;
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
            if (_dragging && _dragBlock != null)
            {
                graphicsDevice.Viewport = _layout.Window;

                Rectangle ghost = new Rectangle(
                    _mousePos.X - _dragGrabOffset.X,
                    _mousePos.Y - _dragGrabOffset.Y,
                    _dragSize.X, _dragSize.Y);

                sb.Begin(samplerState: SamplerState.PointClamp);
                DrawBlockFace(sb, ghost, _dragBlock, 0.85f);
                sb.End();
            }
        }

        private void DrawMiddlePanel(SpriteBatch sb)
        {
            Viewport mid = _layout.Middle;
            Color panelColor = new Color(30, 30, 40);
            Color loopColor = BlockInfo.GetColor(BlockType.Repeat);

            // Panel background
            _ui.Rect(sb, new Rectangle(0, 0, mid.Width, mid.Height), panelColor);

            // ---- Scrolling content: drawn first, then the header and footer are painted over it ----

            // Faint outlines for the empty slots after the last row (as many as blocks you can still add)
            int slotsLeft = MaxBlocks - TotalBlocks();
            for (int k = 0; k < slotsLeft; k++)
            {
                Rectangle outline = new Rectangle(
                    Margin,
                    HeaderHeight + _contentCursor + k * SlotStep - (int)_scrollY,
                    mid.Width - Margin * 2,
                    BlockHeight);

                if (outline.Y >= HeaderHeight + ListHeight) break;   // below the view: nothing more to draw
                if (IsInView(outline))
                    _ui.Border(sb, outline, 1, new Color(60, 60, 75));
            }

            // The coloured spine down the left side of each loop's children
            foreach (Row row in _rows)
            {
                if (row.Kind != RowKind.Block || !row.Block.IsLoop) continue;

                Rectangle header = RowRect(row);
                Rectangle end = RowRect(_rows[row.EndIndex]);
                Rectangle spine = new Rectangle(header.X, header.Bottom, Indent - 4, end.Y - header.Bottom);

                if (spine.Height > 0 && IsInView(spine))
                    _ui.Rect(sb, spine, loopColor);
            }

            // The rows themselves (the command being executed gets a yellow outline)
            foreach (Row row in _rows)
            {
                Rectangle rect = RowRect(row);
                if (!IsInView(rect)) continue;

                if (row.Kind == RowKind.LoopEnd)
                {
                    _ui.Rect(sb, rect, loopColor);
                    _ui.Border(sb, rect, 2, Color.White);
                }
                else
                {
                    DrawBlockFace(sb, rect, row.Block, 1f);

                    if (_runner.IsRunning && row.Block == _runner.ActiveBlock)
                        _ui.Border(sb, rect, 4, Color.Yellow);
                }
            }

            // Line showing where the dragged block will land: yellow if it fits, red if it doesn't
            if (_dragging && mid.Bounds.Contains(_mousePos))
            {
                DropTarget target = GetDropTarget(ToLocal(_mousePos, mid));
                int indent = target.Depth * Indent;

                Rectangle line = new Rectangle(
                    Margin + indent,
                    HeaderHeight + target.LineY - (int)_scrollY - BlockGap / 2 - 1,
                    mid.Width - Margin * 2 - indent,
                    3);

                _ui.Rect(sb, line, CanDrop(target) ? Color.Yellow : new Color(255, 90, 90));
            }

            // ---- Header and footer hide any row that has scrolled underneath them ----
            _ui.Rect(sb, new Rectangle(0, 0, mid.Width, HeaderHeight), panelColor);
            _ui.Rect(sb, new Rectangle(0, mid.Height - FooterHeight, mid.Width, FooterHeight), panelColor);

            // Divider line on the left edge
            _ui.Rect(sb, new Rectangle(0, 0, 2, mid.Height), Color.White);

            _ui.Text(sb, "Code Queue", new Vector2(Margin, 10), Color.White);

            // Block counter on the right of the header: "Blocks 4/6" (amber once the limit is reached)
            int total = TotalBlocks();
            string counter = "Blocks " + total + "/" + MaxBlocks;
            Vector2 counterSize = _ui.Font.MeasureString(counter);
            Color counterColor = total >= MaxBlocks ? new Color(255, 200, 100) : Color.White;
            _ui.Text(sb, counter, new Vector2(mid.Width - Margin - counterSize.X, 10), counterColor);

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

        // Draws a block's face: a Repeat block gets its number box, everything else a plain label
        private void DrawBlockFace(SpriteBatch sb, Rectangle rect, Block block, float alpha)
        {
            if (!block.IsLoop)
            {
                _ui.Block(sb, rect, block.Type, alpha);
                return;
            }

            _ui.Rect(sb, rect, BlockInfo.GetColor(BlockType.Repeat) * alpha);
            _ui.Border(sb, rect, 2, Color.White * alpha);

            Rectangle box = CountBoxRect(rect);
            bool editing = _editBlock == block;

            // "Repeat [ 3 ] times"
            Rectangle labelArea = new Rectangle(rect.X + 8, rect.Y, Math.Max(0, box.X - rect.X - 8), rect.Height);
            _ui.FittedText(sb, "Repeat", labelArea, Color.White * alpha, false);

            _ui.Rect(sb, box, (editing ? Color.White : new Color(20, 20, 30)) * alpha);
            _ui.Border(sb, box, 2, (editing ? Color.Yellow : Color.White) * alpha);

            string number = editing ? _editText + "|" : block.Count.ToString();
            _ui.FittedText(sb, number, box, (editing ? Color.Black : Color.White) * alpha, true);

            Rectangle timesArea = new Rectangle(box.Right + 6, rect.Y, Math.Max(0, rect.Right - box.Right - 10), rect.Height);
            _ui.FittedText(sb, "times", timesArea, Color.White * alpha, false);
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

            // Each available block, with a small number showing how many more can be placed.
            // At zero the block is dimmed and can't be picked up.
            for (int i = 0; i < _palette.Count; i++)
            {
                BlockType type = _palette[i];
                Rectangle slot = PaletteSlot(i);
                int remaining = RemainingOf(type);

                _ui.Block(sb, slot, type, remaining > 0 ? 1f : 0.35f);
                DrawCountBadge(sb, slot, remaining);
            }
        }

        // The small number box at the right end of a palette block
        private void DrawCountBadge(SpriteBatch sb, Rectangle slot, int remaining)
        {
            Rectangle badge = new Rectangle(
                slot.Right - BadgeSize - 8,
                slot.Y + (slot.Height - BadgeSize) / 2,
                BadgeSize,
                BadgeSize);

            Color color = remaining > 0 ? Color.White : new Color(255, 110, 110);

            _ui.Rect(sb, badge, new Color(20, 20, 30));
            _ui.Border(sb, badge, 2, color);
            _ui.FittedText(sb, remaining.ToString(), badge, color, true);
        }
    }
}