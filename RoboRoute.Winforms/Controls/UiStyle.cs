namespace RoboRoute.Winforms.Controls;

// Colours, the font and the block painting shared by the palette items and the queue blocks.
internal static class UiStyle
{
    public static readonly Color QueueBack = Color.FromArgb(24, 24, 32);
    public static readonly Font BlockFont = new Font("Segoe UI", 10f, FontStyle.Bold);

    // A colour part of the way from one colour to another (amount 0 = "from", 1 = "to")
    public static Color Blend(Color from, Color to, float amount)
    {
        return Color.FromArgb(
            (int)(from.R + (to.R - from.R) * amount),
            (int)(from.G + (to.G - from.G) * amount),
            (int)(from.B + (to.B - from.B) * amount));
    }

    // Paints a rectangle as a code block: fill colour, border, and an optional centred label.
    // "active" adds the thick yellow outline for the block that is running; "dim" greys the text and border.
    public static void PaintBlock(Graphics g, Rectangle r, Color fill, string? label, bool active, bool dim)
    {
        using (var brush = new SolidBrush(fill))
            g.FillRectangle(brush, r);

        using (var pen = new Pen(dim ? Color.Gray : Color.White, 2f))
            g.DrawRectangle(pen, r.X + 1, r.Y + 1, r.Width - 3, r.Height - 3);

        if (active)
        {
            using (var pen = new Pen(Color.Yellow, 4f))
                g.DrawRectangle(pen, r.X + 2, r.Y + 2, r.Width - 5, r.Height - 5);
        }

        if (label != null)
        {
            TextRenderer.DrawText(
                g, label, BlockFont, r, dim ? Color.Silver : Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}
