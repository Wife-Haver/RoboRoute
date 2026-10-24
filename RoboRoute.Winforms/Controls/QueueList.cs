using System.Drawing.Drawing2D;
using RoboRoute.Winforms.Core;

namespace RoboRoute.Winforms.Controls;

// A vertical stack of BlockViews for one list of blocks: the whole queue (the "root"),
// or the inside of a Repeat block. Layout is done by hand in DoLayout so nested loops size correctly.
public class QueueList : Panel
{
    public const int Gap = 6;              // vertical space between blocks
    public const int EmptyHeight = 30;     // height of an empty loop body (so it can still be dropped into)

    private readonly List<BlockView> _views = new List<BlockView>();

    // The blocks this list shows. For the root this is the ProgramRunner's queue; for a loop it is Block.Children.
    public List<Block> Model { get; }

    // The Repeat block this list is the inside of (null for the root list)
    public Block? OwnerBlock { get; }

    // The list that contains the owning Repeat block (null for the root list)
    public QueueList? ParentList { get; }

    // How many Repeat blocks enclose this list: 0 for the root, 1 inside a loop, 2 inside a loop in a loop...
    public int Depth { get; }

    public IReadOnlyList<BlockView> Views => _views;

    public QueueList(List<Block> model, Block? ownerBlock, QueueList? parentList)
    {
        Model = model;
        OwnerBlock = ownerBlock;
        ParentList = parentList;
        Depth = parentList == null ? 0 : parentList.Depth + 1;

        BackColor = UiStyle.QueueBack;
        Cursor = Cursors.Default;
        DoubleBuffered = true;
    }

    public void AddView(BlockView view)
    {
        _views.Add(view);
        Controls.Add(view);
    }

    // Removes and disposes every block view (loops dispose the views inside them too)
    public void ClearViews()
    {
        foreach (BlockView view in _views)
        {
            Controls.Remove(view);
            view.Dispose();
        }
        _views.Clear();
    }

    // Positions every block top to bottom for the given width and returns the height this list needs.
    // The caller sets this list's own size.
    public int DoLayout(int width)
    {
        int y = 0;

        foreach (BlockView view in _views)
        {
            int height = view.DoLayout(width);
            view.SetBounds(0, y, width, height);
            y += height + Gap;
        }

        if (_views.Count == 0)
            return OwnerBlock == null ? 0 : EmptyHeight;

        return y - Gap;
    }

    // Hints shown while a list is empty
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_views.Count > 0) return;

        Color hint = Color.FromArgb(130, 130, 150);

        if (OwnerBlock != null)
        {
            using var pen = new Pen(Color.FromArgb(90, 90, 110)) { DashStyle = DashStyle.Dash };
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            TextRenderer.DrawText(
                e.Graphics, "drop blocks here", Font, ClientRectangle, hint,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        else
        {
            var area = new Rectangle(0, 0, Width, Math.Min(Height, 120));
            TextRenderer.DrawText(
                e.Graphics, "Drag blocks from the palette into here", Font, area, hint,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
