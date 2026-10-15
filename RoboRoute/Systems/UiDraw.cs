using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoboRoute.RoboRoute.Commands;
using System;

namespace RoboRoute
{
    // Shared drawing helpers for all the UI: rectangles, borders, fitted text, code blocks.
    // Owns the 1x1 white texture and the font so nothing else has to.
    public class UiDraw
    {
        private readonly Texture2D _pixel;

        public SpriteFont Font { get; }

        public UiDraw(GraphicsDevice graphicsDevice, SpriteFont font)
        {
            Font = font;

            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Rect(SpriteBatch sb, Rectangle r, Color color)
        {
            sb.Draw(_pixel, r, color);
        }

        public void Border(SpriteBatch sb, Rectangle r, int thickness, Color color)
        {
            sb.Draw(_pixel, new Rectangle(r.X, r.Y, r.Width, thickness), color);                    // top
            sb.Draw(_pixel, new Rectangle(r.X, r.Bottom - thickness, r.Width, thickness), color);   // bottom
            sb.Draw(_pixel, new Rectangle(r.X, r.Y, thickness, r.Height), color);                   // left
            sb.Draw(_pixel, new Rectangle(r.Right - thickness, r.Y, thickness, r.Height), color);   // right
        }

        public void Text(SpriteBatch sb, string text, Vector2 position, Color color)
        {
            sb.DrawString(Font, text, position, color);
        }

        // Draws text inside an area, shrinking it if it's too wide. Centred, or left-aligned.
        public void FittedText(SpriteBatch sb, string text, Rectangle area, Color color, bool center)
        {
            Vector2 size = Font.MeasureString(text);
            float textScale = Math.Min(1f, (area.Width - (center ? 12 : 0)) / size.X);

            float x = center ? area.X + (area.Width - size.X * textScale) / 2f : area.X;
            float y = area.Y + (area.Height - size.Y * textScale) / 2f;

            sb.DrawString(Font, text, new Vector2(x, y), color, 0f, Vector2.Zero, textScale, SpriteEffects.None, 0f);
        }

        // Draws one code block: coloured box, border, and a centred label that shrinks to fit
        public void Block(SpriteBatch sb, Rectangle r, BlockType type, float alpha = 1f)
        {
            Rect(sb, r, BlockInfo.GetColor(type) * alpha);
            Border(sb, r, 2, Color.White * alpha);
            FittedText(sb, BlockInfo.GetLabel(type), r, Color.White * alpha, true);
        }
    }
}