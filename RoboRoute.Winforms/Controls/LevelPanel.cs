using System.Drawing.Drawing2D;
using RoboRoute.Winforms.Core;

namespace RoboRoute.Winforms.Controls;

// Draws the level: the tile map scaled to fit the panel, with the robot on top.
// It owns the images and only paints; all the game logic stays in Core.
public class LevelPanel : Panel
{
    private const int EdgeGap = 12;            // empty space around the map, in pixels
    private const float RobotFill = 0.8f;      // fraction of a tile the robot sprite fills
    private const string RobotImageName = "Robot";

    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };

    // Image file name for each tile ID (index = tile ID).
    // To add a tile type: add its ID in LevelTileMap, then its image name and fallback colour here.
    private static readonly string[] TileImageNames =
    {
        "FloorTile",    // Floor
        "WallTile",     // Wall
        "StartTile",    // Start
        "GoalTile",     // Goal
        "HazardTile",   // Hazard
    };

    // Solid colours drawn instead of an image that couldn't be found
    private static readonly Color[] TileFallbackColors =
    {
        Color.FromArgb(120, 120, 130),   // floor
        Color.FromArgb(50, 50, 60),      // wall
        Color.FromArgb(70, 120, 200),    // start
        Color.FromArgb(70, 190, 110),    // goal
        Color.FromArgb(210, 70, 70),     // hazard
    };

    private readonly Image?[] _tileImages = new Image?[TileImageNames.Length];
    private Image? _robotImage;

    private LevelTileMap? _map;
    private Robot? _robot;

    // Names of images that weren't found by LoadAssets (placeholders are drawn for these)
    public List<string> MissingAssets { get; } = new List<string>();

    public LevelPanel()
    {
        DoubleBuffered = true;      // stops flicker while the robot animates
        ResizeRedraw = true;        // repaint whenever the panel is resized
        BackColor = Color.CornflowerBlue;
    }

    // Loads every tile image and the robot image from a folder.
    // A missing file isn't an error: it's listed in MissingAssets and a placeholder is drawn instead.
    public void LoadAssets(string folder)
    {
        DisposeImages();
        MissingAssets.Clear();

        for (int i = 0; i < TileImageNames.Length; i++)
            _tileImages[i] = LoadImage(folder, TileImageNames[i]);

        _robotImage = LoadImage(folder, RobotImageName);
        Invalidate();
    }

    // Tells the panel what to draw. Call this again whenever a new level is loaded.
    public void SetLevel(LevelTileMap map, Robot robot)
    {
        _map = map;
        _robot = robot;
        Invalidate();
    }

    private Image? LoadImage(string folder, string name)
    {
        foreach (string extension in ImageExtensions)
        {
            string path = Path.Combine(folder, name + extension);
            if (!File.Exists(path)) continue;

            // Copy into a new Bitmap so the file on disk isn't left locked
            using Image original = Image.FromFile(path);
            return new Bitmap(original);
        }

        MissingAssets.Add(name);
        return null;
    }

    // ------------------------------------------------------------------
    //  Painting
    // ------------------------------------------------------------------

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (DesignMode || _map == null || _robot == null) return;

        Graphics g = e.Graphics;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;   // keeps pixel art crisp
        g.PixelOffsetMode = PixelOffsetMode.Half;

        int tileSize = LevelTileMap.TileSize;

        // Scale the whole map so it fits inside the panel, centred
        float availableW = Math.Max(0, ClientSize.Width - EdgeGap * 2);
        float availableH = Math.Max(0, ClientSize.Height - EdgeGap * 2);
        float scale = Math.Min(availableW / (_map.Width * tileSize), availableH / (_map.Height * tileSize));
        if (scale <= 0) return;

        float tilePx = tileSize * scale;
        float offsetX = (ClientSize.Width - _map.Width * tilePx) / 2f;
        float offsetY = (ClientSize.Height - _map.Height * tilePx) / 2f;

        // Tiles. Each edge is rounded on its own so neighbouring tiles meet with no gaps.
        for (int y = 0; y < _map.Height; y++)
        {
            for (int x = 0; x < _map.Width; x++)
            {
                Rectangle rect = Rectangle.FromLTRB(
                    (int)MathF.Round(offsetX + x * tilePx),
                    (int)MathF.Round(offsetY + y * tilePx),
                    (int)MathF.Round(offsetX + (x + 1) * tilePx),
                    (int)MathF.Round(offsetY + (y + 1) * tilePx));

                int id = _map.GetTile(new Point(x, y));

                // Start, Goal and Hazard sit on top of a floor tile, so their art can have transparent edges
                if (id == LevelTileMap.Start || id == LevelTileMap.Goal || id == LevelTileMap.Hazard)
                    DrawTile(g, LevelTileMap.Floor, rect);

                DrawTile(g, id, rect);
            }
        }

        // Robot, centred on its (possibly mid-move) position
        var pos = _robot.PixelPos;
        float centerX = offsetX + (pos.X + tileSize / 2f) * scale;
        float centerY = offsetY + (pos.Y + tileSize / 2f) * scale;
        DrawRobot(g, centerX, centerY, tilePx * RobotFill, _robot.Angle * 180f / MathF.PI);
    }

    private void DrawTile(Graphics g, int id, Rectangle rect)
    {
        Image? image = id >= 0 && id < _tileImages.Length ? _tileImages[id] : null;

        if (image != null)
        {
            g.DrawImage(image, rect);
            return;
        }

        Color fallback = id >= 0 && id < TileFallbackColors.Length ? TileFallbackColors[id] : Color.Magenta;
        using var brush = new SolidBrush(fallback);
        g.FillRectangle(brush, rect);
    }

    // Draws the robot rotated around its own centre. "size" is the length of its longest side in pixels.
    private void DrawRobot(Graphics g, float centerX, float centerY, float size, float angleDegrees)
    {
        GraphicsState state = g.Save();

        g.TranslateTransform(centerX, centerY);
        g.RotateTransform(angleDegrees);      // positive = clockwise, same as the old MonoGame version

        if (_robotImage != null)
        {
            float fit = size / Math.Max(_robotImage.Width, _robotImage.Height);
            float w = _robotImage.Width * fit;
            float h = _robotImage.Height * fit;
            g.DrawImage(_robotImage, -w / 2f, -h / 2f, w, h);
        }
        else
        {
            // Placeholder: an orange disc with a white arrow pointing the way it faces
            using var body = new SolidBrush(Color.Orange);
            using var nose = new SolidBrush(Color.White);
            g.FillEllipse(body, -size / 2f, -size / 2f, size, size);

            PointF[] arrow =
            {
                new(0f, -size / 2f),
                new(-size / 5f, -size / 6f),
                new(size / 5f, -size / 6f),
            };
            g.FillPolygon(nose, arrow);
        }

        g.Restore(state);
    }

    // ------------------------------------------------------------------
    //  Cleanup
    // ------------------------------------------------------------------

    private void DisposeImages()
    {
        for (int i = 0; i < _tileImages.Length; i++)
        {
            _tileImages[i]?.Dispose();
            _tileImages[i] = null;
        }

        _robotImage?.Dispose();
        _robotImage = null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            DisposeImages();

        base.Dispose(disposing);
    }
}
