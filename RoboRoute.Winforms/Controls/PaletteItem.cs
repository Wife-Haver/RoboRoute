using RoboRoute.Winforms.Core;

namespace RoboRoute.Winforms.Controls;

// One entry in the block palette: a coloured block with a small badge showing how many more can be placed.
// Dragging it asks the CodeEditor (through DragRequested) to start a drag with a brand-new block.
public class PaletteItem : Control
{
    private const int BadgeSize = 26;

    private int _remaining;
    private bool _pressed;
    private Point _mouseDownAt;

    public BlockType Type { get; }

    // How many more of this block the player may place. At zero the item is greyed out.
    public int Remaining
    {
        get => _remaining;
        set
        {
            if (_remaining == value) return;
            _remaining = value;
            Invalidate();
        }
    }

    // Raised once the mouse has moved far enough with the button held to count as a drag
    public event Action<PaletteItem>? DragRequested;

    public PaletteItem(BlockType type)
    {
        Type = type;
        DoubleBuffered = true;
        Height = 40;
        Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        bool available = _remaining > 0;

        Color fill = BlockInfo.GetColor(Type);
        if (!available)
            fill = UiStyle.Blend(fill, BackColor, 0.65f);

        // The label gets the space to the left of the badge
        var face = new Rectangle(0, 0, Width, Height);
        UiStyle.PaintBlock(g, face, fill, null, false, !available);
        TextRenderer.DrawText(
            g, BlockInfo.GetLabel(Type), UiStyle.BlockFont,
            new Rectangle(8, 0, Math.Max(0, Width - BadgeSize - 24), Height),
            available ? Color.White : Color.Silver,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

        // Badge with the remaining count
        var badge = new Rectangle(Width - BadgeSize - 8, (Height - BadgeSize) / 2, BadgeSize, BadgeSize);
        Color badgeColor = available ? Color.White : Color.FromArgb(255, 110, 110);
        using (var brush = new SolidBrush(Color.FromArgb(20, 20, 30)))
            g.FillRectangle(brush, badge);
        using (var pen = new Pen(badgeColor, 2f))
            g.DrawRectangle(pen, badge.X + 1, badge.Y + 1, badge.Width - 3, badge.Height - 3);
        TextRenderer.DrawText(
            g, _remaining.ToString(), UiStyle.BlockFont, badge, badgeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    // ---- Start of a drag: the mouse button goes down, then the mouse moves a little ----

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button == MouseButtons.Left)
        {
            _pressed = true;
            _mouseDownAt = e.Location;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_pressed || e.Button != MouseButtons.Left) return;

        Size threshold = SystemInformation.DragSize;
        if (Math.Abs(e.X - _mouseDownAt.X) < threshold.Width / 2 &&
            Math.Abs(e.Y - _mouseDownAt.Y) < threshold.Height / 2)
            return;

        _pressed = false;
        DragRequested?.Invoke(this);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _pressed = false;
    }
}
