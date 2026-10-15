using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RoboRoute
{
    // Splits the window into three side-by-side viewports: Level | Middle | Right.
    // Owns the two ratios and the keys that adjust them.
    public class ViewportLayout
    {
        // Fraction of the window WIDTH given to the level (left viewport)
        private const float MinLevelRatio = 0.5f;   // level always >= 50%, so it's always the biggest
        private const float MaxLevelRatio = 0.7f;

        // How the REMAINING width is split between the middle and right viewports (0..1 = share for middle)
        private const float MinMiddleShare = 0.2f;
        private const float MaxMiddleShare = 0.8f;

        private float _levelRatio = 0.5f;
        private float _middleShare = 0.5f;

        private int _width;
        private int _height;

        public Viewport Window { get; private set; }   // the whole backbuffer
        public Viewport Level { get; private set; }
        public Viewport Middle { get; private set; }
        public Viewport Right { get; private set; }

        // Call every frame with the current backbuffer size; only recalculates when it changed
        public void SetWindowSize(int width, int height)
        {
            if (width == _width && height == _height) return;

            _width = width;
            _height = height;
            Recalculate();
        }

        public void Update(KeyboardState kb, float dt)
        {
            float oldLevel = _levelRatio;
            float oldShare = _middleShare;

            // [ and ] : shrink / grow the level viewport
            if (kb.IsKeyDown(Keys.OemOpenBrackets)) _levelRatio -= 0.3f * dt;
            if (kb.IsKeyDown(Keys.OemCloseBrackets)) _levelRatio += 0.3f * dt;

            // , and . : move the divider between the middle and right viewports
            if (kb.IsKeyDown(Keys.OemComma)) _middleShare -= 0.5f * dt;
            if (kb.IsKeyDown(Keys.OemPeriod)) _middleShare += 0.5f * dt;

            if (_levelRatio != oldLevel || _middleShare != oldShare)
                Recalculate();
        }

        private void Recalculate()
        {
            _levelRatio = MathHelper.Clamp(_levelRatio, MinLevelRatio, MaxLevelRatio);
            _middleShare = MathHelper.Clamp(_middleShare, MinMiddleShare, MaxMiddleShare);

            // Work in whole pixels so the three viewports always add up to the window width exactly
            int levelWidth = (int)(_width * _levelRatio);
            int remaining = _width - levelWidth;
            int middleWidth = (int)(remaining * _middleShare);
            int rightWidth = remaining - middleWidth;

            Window = new Viewport(0, 0, _width, _height);
            Level = new Viewport(0, 0, levelWidth, _height);
            Middle = new Viewport(levelWidth, 0, middleWidth, _height);
            Right = new Viewport(levelWidth + middleWidth, 0, rightWidth, _height);
        }
    }
}