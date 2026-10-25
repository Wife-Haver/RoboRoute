using RoboRoute.Winforms.Core;

namespace RoboRoute.Winforms.Controls;

// One block in the code queue. A normal block is just a painted rectangle.
// A Repeat block also hosts a number box (NumericUpDown) and a QueueList for the blocks inside it.
public class BlockView : Panel
{
    public const int BlockHeight = 40;
    public const int HeaderHeight = 40;     // the top bar of a Repeat block
    public const int FooterHeight = 18;     // the closing bar of a Repeat block
    public const int LoopIndent = 16;       // how far the blocks inside a loop are pushed right
    public const int ChildPad = 4;          // space above and below the blocks inside a loop

    private readonly NumericUpDown? _countBox;
    private int _labelWidth;
    private int _childHeight;
    private bool _isActive;
    private bool _locked;
    private bool _pressed;
    private Point _mouseDownAt;

    public Block Model { get; }

    // The list this block sits in
    public QueueList OwnerList { get; }

    // The blocks inside this one (Repeat blocks only)
    public QueueList? Children { get; }

    // True while this block is the one the robot is executing (draws the yellow outline)
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value) return;
            _isActive = value;
            Invalidate();
        }
    }

    // While the program runs the number box can't be edited
    public bool Locked
    {
        get => _locked;
        set
        {
            _locked = value;
            if (_countBox != null)
                _countBox.Enabled = !value;
        }
    }

    // Raised once the mouse has moved far enough with the button held to count as a drag
    public event Action<BlockView>? DragRequested;

    // Raised when the player changes a Repeat block's number
    public event Action? CountChanged;

    public BlockView(Block model, QueueList ownerList)
    {
        Model = model;
        OwnerList = ownerList;

        BackColor = UiStyle.QueueBack;
        DoubleBuffered = true;
        Cursor = Cursors.Hand;

        // Only a Repeat block has anything inside it
        if (model is not RepeatBlock repeat) return;

        Children = new QueueList(repeat.Body, model, ownerList);
        Controls.Add(Children);

        _countBox = new NumericUpDown
        {
            Minimum = RepeatBlock.MinCount,
            Maximum = RepeatBlock.MaxCount,
            Value = Math.Clamp(repeat.Count, RepeatBlock.MinCount, RepeatBlock.MaxCount),
            Width = 56,
            TextAlign = HorizontalAlignment.Center,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Default
        };
        _countBox.ValueChanged += (sender, e) =>
        {
            repeat.Count = (int)_countBox.Value;
            CountChanged?.Invoke();
        };
        Controls.Add(_countBox);
    }

    // ------------------------------------------------------------------
    //  Layout
    // ------------------------------------------------------------------

    // Positions everything inside this block for the given width and returns the height it needs.
    // The caller sets this block's own bounds.
    public int DoLayout(int width)
    {
        if (Children == null || _countBox == null)
            return BlockHeight;

        _labelWidth = TextRenderer.MeasureText("Repeat", UiStyle.BlockFont).Width;
        _countBox.Location = new Point(8 + _labelWidth + 8, (HeaderHeight - _countBox.Height) / 2);

        int childWidth = Math.Max(10, width - LoopIndent);
        _childHeight = Children.DoLayout(childWidth);
        Children.SetBounds(LoopIndent, HeaderHeight + ChildPad, childWidth, _childHeight);

        return HeaderHeight + ChildPad + _childHeight + ChildPad + FooterHeight;
    }

    // ------------------------------------------------------------------
    //  Painting
    // ------------------------------------------------------------------

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        Color color = Model.FillColor;

        if (!Model.IsLoop)
        {
            UiStyle.PaintBlock(g, new Rectangle(0, 0, Width, BlockHeight), color, Model.DisplayName, _isActive, false);
            return;
        }

        // Top bar: "Repeat [ 3 ] times"
        UiStyle.PaintBlock(g, new Rectangle(0, 0, Width, HeaderHeight), color, null, false, false);
        TextRenderer.DrawText(
            g, "Repeat", UiStyle.BlockFont,
            new Rectangle(8, 0, _labelWidth + 4, HeaderHeight), Color.White,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);

        if (_countBox != null)
        {
            TextRenderer.DrawText(
                g, "times", UiStyle.BlockFont,
                new Rectangle(_countBox.Right + 6, 0, Math.Max(0, Width - _countBox.Right - 10), HeaderHeight), Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPadding);
        }

        // Coloured bar down the left side of the blocks inside, then the closing bar
        using (var brush = new SolidBrush(color))
            g.FillRectangle(brush, 0, HeaderHeight, LoopIndent - 4, _childHeight + ChildPad * 2);

        int footerTop = HeaderHeight + ChildPad + _childHeight + ChildPad;
        UiStyle.PaintBlock(g, new Rectangle(0, footerTop, Width, FooterHeight), color, null, false, false);
    }

    // ------------------------------------------------------------------
    //  Start of a drag: the mouse button goes down, then the mouse moves a little
    // ------------------------------------------------------------------

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

        if (!_pressed || e.Button != MouseButtons.Left || _locked) return;

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
