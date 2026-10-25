using RoboRoute.Winforms.Core;

namespace RoboRoute.Winforms.Controls;

// Connects the block palette and the code queue to the game.
//
//  - It builds the palette items and the queue views, and keeps them in step with the model
//    (the tree of Blocks in ProgramRunner.Queue, which is the source of truth).
//  - It runs the drag-and-drop: the drop handlers only change the model, and the views are rebuilt afterwards.
//  - It enforces the level's limits (total blocks, how many of each type, loop nesting).
//
// MainForm creates one CodeEditor per level and calls Update() on every timer tick.
public class CodeEditor : IDisposable
{
    private const int MaxLoopNesting = 3;
    private const int PaletteMargin = 8;
    private const int PaletteGap = 6;
    private const int EdgeScrollZone = 30;     // pixels from the top/bottom of the queue where dragging auto-scrolls
    private const int EdgeScrollStep = 18;     // pixels scrolled per drag event

    // What is being dragged. SourceList is null for a brand-new block from the palette;
    // otherwise it is the model list the block is currently in (it stays there until the drop succeeds).
    private sealed record DragPayload(Block Block, List<Block>? SourceList);

    // Where a dropped block would go: this list, at this position
    private sealed record DropTarget(QueueList List, int Index);

    private readonly ProgramRunner _runner;
    private readonly LevelData _level;
    private readonly Panel _queueHost;
    private readonly Panel _paletteHost;
    private readonly QueueList _root;
    private readonly Panel _marker = new Panel { Height = 3, Visible = false, Enabled = false };
    private readonly List<PaletteItem> _paletteItems = new List<PaletteItem>();
    private readonly Dictionary<Block, BlockView> _viewOf = new Dictionary<Block, BlockView>();

    private DragPayload? _drag;
    private bool _modelChanged;
    private bool _layingOut;
    private bool _disposed;
    private bool _lastRunning;
    private Block? _lastActive;

    public CodeEditor(ProgramRunner runner, LevelData level, Panel queueHost, Panel paletteHost)
    {
        _runner = runner;
        _level = level;
        _queueHost = queueHost;
        _paletteHost = paletteHost;

        // ---- Queue side: a scrolling host containing the root list ----
        ClearHost(_queueHost);
        _queueHost.AutoScroll = true;
        _queueHost.BackColor = UiStyle.QueueBack;

        _root = new QueueList(_runner.Queue, null, null);
        _queueHost.Controls.Add(_root);
        _root.Controls.Add(_marker);
        AttachDragHandlers(_root);   // also covers the marker, which is inside the root
        _queueHost.ClientSizeChanged += OnQueueHostSizeChanged;

        // ---- Palette side: one item for every block type this level gives ----
        ClearHost(_paletteHost);

        int y = PaletteMargin;
        foreach (BlockType type in Enum.GetValues<BlockType>())
        {
            if (_level.GetGivenCount(type) <= 0) continue;

            var item = new PaletteItem(type)
            {
                Location = new Point(PaletteMargin, y),
                //Width = Math.Max(60, _paletteHost.ClientSize.Width - PaletteMargin * 2),
                //Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            item.DragRequested += StartDragFromPalette;
            _paletteHost.Controls.Add(item);
            _paletteItems.Add(item);

            y += item.Height + PaletteGap;
        }

        LayoutPalette();
        _paletteHost.ClientSizeChanged += OnPaletteHostSizeChanged;

        // Dropping a block back onto the palette throws it away
        AttachDragHandlers(_paletteHost);

        Rebuild();
    }
    private void OnPaletteHostSizeChanged(object? sender, EventArgs e)
    {
        LayoutPalette();
    }

    // Makes every palette block as wide as the palette panel currently is
    private void LayoutPalette()
    {
        int width = Math.Max(60, _paletteHost.ClientSize.Width - PaletteMargin * 2);

        foreach (PaletteItem item in _paletteItems)
            item.Width = width;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _queueHost.ClientSizeChanged -= OnQueueHostSizeChanged;

        // The palette host outlives this editor, so take our handlers off it
        _paletteHost.AllowDrop = false;
        _paletteHost.DragEnter -= OnDragOver;
        _paletteHost.DragOver -= OnDragOver;
        _paletteHost.DragLeave -= OnDragLeave;
        _paletteHost.DragDrop -= OnDragDrop;
        _paletteHost.ClientSizeChanged -= OnPaletteHostSizeChanged;

        ClearHost(_queueHost);
        ClearHost(_paletteHost);
    }

    private static void ClearHost(Panel host)
    {
        for (int i = host.Controls.Count - 1; i >= 0; i--)
        {
            Control control = host.Controls[i];
            host.Controls.RemoveAt(i);
            control.Dispose();
        }
    }

    // ------------------------------------------------------------------
    //  Keeping the views in step with the game (called every timer tick)
    // ------------------------------------------------------------------

    public void Update()
    {
        if (_disposed) return;

        bool running = _runner.IsRunning;
        if (running != _lastRunning)
        {
            _lastRunning = running;
            ApplyLock();
        }

        // Outline the block the robot is executing, and keep it in view
        Block? active = running ? _runner.ActiveBlock : null;
        if (!ReferenceEquals(active, _lastActive))
        {
            if (_lastActive != null && _viewOf.TryGetValue(_lastActive, out BlockView? old))
                old.IsActive = false;

            if (active != null && _viewOf.TryGetValue(active, out BlockView? current))
            {
                current.IsActive = true;
                _queueHost.ScrollControlIntoView(current);
            }

            _lastActive = active;
        }
    }

    private void ApplyLock()
    {
        foreach (BlockView view in _viewOf.Values)
            view.Locked = _runner.IsRunning;
    }

    // ------------------------------------------------------------------
    //  Building the views from the model
    // ------------------------------------------------------------------

    private void Rebuild()
    {
        if (_disposed) return;

        int scroll = _queueHost.VerticalScroll.Value;

        _root.ClearViews();
        _viewOf.Clear();
        BuildViews(_root);

        // Hook the drag events onto every control that was just created
        foreach (BlockView view in _root.Views)
            AttachDragHandlers(view);

        ApplyLock();
        RelayoutAll();
        _queueHost.AutoScrollPosition = new Point(0, scroll);

        RefreshPalette();

        _marker.Visible = false;
        _lastActive = null;
    }

    private void BuildViews(QueueList list)
    {
        foreach (Block block in list.Model)
        {
            var view = new BlockView(block, list);
            view.DragRequested += StartDragFromQueue;
            view.CountChanged += OnCountChanged;

            list.AddView(view);
            _viewOf[block] = view;

            if (view.Children != null)
                BuildViews(view.Children);
        }
    }

    private void OnCountChanged()
    {
        _runner.ClearStatus();
    }

    private void OnQueueHostSizeChanged(object? sender, EventArgs e)
    {
        RelayoutAll();
    }

    // Sizes and positions every block for the host's current width
    private void RelayoutAll()
    {
        if (_layingOut || _disposed) return;
        _layingOut = true;

        try
        {
            // A second pass is needed when the scroll bar appears or disappears, because that changes the width
            for (int pass = 0; pass < 2; pass++)
            {
                int width = _queueHost.ClientSize.Width;
                if (width < 40) return;

                int contentHeight = _root.DoLayout(width);

                // Always at least as tall as the host, with a spare block's height at the bottom,
                // so the empty space below the last block is still part of the list (drop there = add to the end)
                int height = Math.Max(contentHeight + BlockView.BlockHeight, _queueHost.ClientSize.Height);
                _root.Size = new Size(width, height);

                if (_queueHost.ClientSize.Width == width) break;
            }
        }
        finally
        {
            _layingOut = false;
        }
    }

    // The number on each palette item: how many more of that block the player may place
    private void RefreshPalette()
    {
        foreach (PaletteItem item in _paletteItems)
        {
            int used = CountType(_runner.Queue, item.Type, null);
            item.Remaining = Math.Max(0, _level.GetGivenCount(item.Type) - used);
        }
    }

    // ------------------------------------------------------------------
    //  Counting blocks (used for the limits)
    // ------------------------------------------------------------------

    // All blocks in a list, loop contents included, skipping "exclude" and everything inside it
    private static int CountTotal(IReadOnlyList<Block> list, Block? exclude)
    {
        int total = 0;
        foreach (Block block in list)
        {
            if (ReferenceEquals(block, exclude)) continue;
            total += 1 + CountTotal(block.Children, exclude);
        }
        return total;
    }

    // Blocks of one type in a list, loop contents included, skipping "exclude" and everything inside it
    private static int CountType(IReadOnlyList<Block> list, BlockType type, Block? exclude)
    {
        int count = 0;
        foreach (Block block in list)
        {
            if (ReferenceEquals(block, exclude)) continue;
            if (block.Type == type) count++;
            count += CountType(block.Children, type, exclude);
        }
        return count;
    }

    // Blocks of one type in a single block and everything inside it
    private static int CountType(Block block, BlockType type)
    {
        int count = block.Type == type ? 1 : 0;
        foreach (Block child in block.Children)
            count += CountType(child, type);
        return count;
    }

    // Is the dragged block (and anything inside it) allowed to go into this list?
    // A block being moved is still in the model, so it is left out of the counts.
    private bool CanDrop(QueueList target, DragPayload payload)
    {
        Block dragged = payload.Block;
        Block? alreadyPlaced = payload.SourceList != null ? dragged : null;

        // A Repeat block can't be dropped into itself or into something inside it
        for (QueueList? list = target; list != null; list = list.ParentList)
        {
            if (ReferenceEquals(list.OwnerBlock, dragged)) return false;
        }

        // Not nested too deeply
        if (target.Depth + dragged.LoopDepth() > MaxLoopNesting) return false;

        // Total block limit for the level
        if (CountTotal(_runner.Queue, alreadyPlaced) + dragged.TotalBlocks() > _level.MaxBlocksAllowed) return false;

        // How many of each type the level gives
        foreach (BlockType type in Enum.GetValues<BlockType>())
        {
            int used = CountType(_runner.Queue, type, alreadyPlaced) + CountType(dragged, type);
            if (used > _level.GetGivenCount(type)) return false;
        }

        return true;
    }

    // ------------------------------------------------------------------
    //  Drag and drop: starting
    // ------------------------------------------------------------------

    private void StartDragFromPalette(PaletteItem item)
    {
        if (_runner.IsRunning || item.Remaining <= 0) return;
        BeginDrag(item, new DragPayload(Block.Create(item.Type), null));
    }

    private void StartDragFromQueue(BlockView view)
    {
        if (_runner.IsRunning) return;
        BeginDrag(view, new DragPayload(view.Model, view.OwnerList.Model));
    }

    private void BeginDrag(Control source, DragPayload payload)
    {
        if (_drag != null) return;

        _drag = payload;
        _runner.ClearStatus();

        try
        {
            // The real data lives in _drag; the string is only there because DoDragDrop needs something
            source.DoDragDrop("RoboRouteBlock", DragDropEffects.Move);
        }
        finally
        {
            _drag = null;
            _marker.Visible = false;
        }

        // The drop handlers only changed the model. Rebuild the views once the handler that started
        // the drag has finished, because it may be the very block that is about to be replaced.
        if (_modelChanged)
        {
            _modelChanged = false;

            if (_queueHost.IsHandleCreated)
                _queueHost.BeginInvoke(new Action(Rebuild));
            else
                Rebuild();
        }
    }

    // ------------------------------------------------------------------
    //  Drag and drop: events
    // ------------------------------------------------------------------

    // The same handlers go on every control a block can be dragged over
    private void AttachDragHandlers(Control control)
    {
        // The number box is skipped: its text box would accept the dropped text instead of the block
        if (control is NumericUpDown) return;

        control.AllowDrop = true;
        control.DragEnter += OnDragOver;
        control.DragOver += OnDragOver;
        control.DragLeave += OnDragLeave;
        control.DragDrop += OnDragDrop;

        foreach (Control child in control.Controls)
            AttachDragHandlers(child);
    }

    private bool IsOverPalette(Point screen)
    {
        return _paletteHost.RectangleToScreen(_paletteHost.ClientRectangle).Contains(screen);
    }

    // The place in the queue a drop at this screen position would go, or null if it isn't over the queue
    private DropTarget? FindTargetAt(Point screen)
    {
        if (!_queueHost.RectangleToScreen(_queueHost.ClientRectangle).Contains(screen))
            return null;

        return FindTarget(_root, screen);
    }

    // Walks down through the loops to find the list and position for a screen point.
    //  - upper half of a block (upper half of a Repeat block's top bar): before that block
    //  - lower half of a Repeat block's top bar, its body, and the upper half of its closing bar: inside the loop
    //  - otherwise: after it, so on to the next block, or the end of this list
    private static DropTarget FindTarget(QueueList list, Point screen)
    {
        IReadOnlyList<BlockView> views = list.Views;

        for (int i = 0; i < views.Count; i++)
        {
            BlockView view = views[i];
            Rectangle rect = view.RectangleToScreen(view.ClientRectangle);

            int beforeLimit = view.Children != null
                ? rect.Top + BlockView.HeaderHeight / 2
                : rect.Top + rect.Height / 2;

            if (screen.Y < beforeLimit)
                return new DropTarget(list, i);

            if (view.Children != null)
            {
                Rectangle body = view.Children.RectangleToScreen(view.Children.ClientRectangle);
                int insideLimit = body.Bottom + BlockView.ChildPad + BlockView.FooterHeight / 2;

                if (screen.Y < insideLimit)
                    return FindTarget(view.Children, screen);
            }
        }

        return new DropTarget(list, views.Count);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (_drag == null)
        {
            e.Effect = DragDropEffects.None;
            return;
        }

        var screen = new Point(e.X, e.Y);    // the drag event positions are screen coordinates

        // Over the palette: dropping throws away a block that came from the queue
        if (IsOverPalette(screen))
        {
            e.Effect = _drag.SourceList != null ? DragDropEffects.Move : DragDropEffects.None;
            _marker.Visible = false;
            return;
        }

        DropTarget? target = FindTargetAt(screen);
        if (target == null)
        {
            e.Effect = DragDropEffects.None;
            _marker.Visible = false;
            return;
        }

        bool allowed = CanDrop(target.List, _drag);
        e.Effect = allowed ? DragDropEffects.Move : DragDropEffects.None;
        ShowMarker(target, allowed);
        AutoScrollIfNeeded(screen);
    }

    private void OnDragLeave(object? sender, EventArgs e)
    {
        _marker.Visible = false;
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        _marker.Visible = false;
        if (_drag == null) return;

        DragPayload payload = _drag;
        var screen = new Point(e.X, e.Y);

        // Dropped on the palette: throw the block away
        if (IsOverPalette(screen))
        {
            if (payload.SourceList != null)
            {
                payload.SourceList.Remove(payload.Block);
                MarkChanged();
            }
            return;
        }

        DropTarget? target = FindTargetAt(screen);
        if (target == null || !CanDrop(target.List, payload)) return;

        int index = target.Index;

        // Moving a block that is already in the queue: take it out of its old place first
        if (payload.SourceList != null)
        {
            int removedAt = payload.SourceList.IndexOf(payload.Block);
            if (removedAt >= 0)
            {
                payload.SourceList.RemoveAt(removedAt);

                // Taking it out above the drop position shifts the position up by one
                if (ReferenceEquals(payload.SourceList, target.List.Model) && removedAt < index)
                    index--;
            }
        }

        target.List.Model.Insert(index, payload.Block);
        MarkChanged();
    }

    private void MarkChanged()
    {
        _modelChanged = true;
        _runner.ClearStatus();
    }

    // The coloured line showing where the block will land: yellow if it fits, red if a limit stops it
    private void ShowMarker(DropTarget target, bool allowed)
    {
        QueueList list = target.List;
        Rectangle listRect = list.RectangleToScreen(list.ClientRectangle);
        int screenY;

        if (target.Index < list.Views.Count)
        {
            BlockView view = list.Views[target.Index];
            screenY = view.RectangleToScreen(view.ClientRectangle).Top - QueueList.Gap / 2;
        }
        else if (list.Views.Count > 0)
        {
            BlockView view = list.Views[list.Views.Count - 1];
            screenY = view.RectangleToScreen(view.ClientRectangle).Bottom + QueueList.Gap / 2;
        }
        else
        {
            screenY = listRect.Top + 2;
        }

        Point position = _root.PointToClient(new Point(listRect.Left, screenY - 1));

        _marker.BackColor = allowed ? Color.Yellow : Color.FromArgb(255, 90, 90);
        _marker.SetBounds(position.X, position.Y, listRect.Width, 3);
        _marker.Visible = true;
        _marker.BringToFront();
    }

    // Holding a block near the top or bottom of the queue scrolls it
    private void AutoScrollIfNeeded(Point screen)
    {
        Rectangle host = _queueHost.RectangleToScreen(_queueHost.ClientRectangle);
        int current = _queueHost.VerticalScroll.Value;
        int wanted = current;

        if (screen.Y < host.Top + EdgeScrollZone)
            wanted -= EdgeScrollStep;
        else if (screen.Y > host.Bottom - EdgeScrollZone)
            wanted += EdgeScrollStep;

        if (wanted != current)
            _queueHost.AutoScrollPosition = new Point(0, Math.Max(0, wanted));
    }
}
