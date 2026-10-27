namespace RoboRoute.Winforms.Screens;

// The "How to play" popup: a picture on top and a block of text underneath.
// The layout is designed in the Visual Studio designer (HowToPlayForm.Designer.cs).
//
// The picture is loaded from Assets/HowToPlay.png (next to the .exe, like the tile images).
// If the picture is missing or can't be read, the popup still opens with the text only.
// To change the text, edit HowToPlayText below (or set txtHelp.Text in the designer).
public partial class HowToPlayForm : Form
{
    private const string ImageFileName = "HowToPlay.png";

    private const string HowToPlayText =
        "Goal: get the robot onto the green goal tile.\r\n" +
        "\r\n" +
        "1. Drag blocks from the palette on the right into the code queue in the middle.\r\n" +
        "2. Press RUN. The robot does your blocks one at a time, from top to bottom.\r\n" +
        "3. If the program ends with the robot on the goal, the level is complete.\r\n" +
        "\r\n" +
        "Blocks: Move Forward moves one tile. Turn Left and Turn Right turn the robot. " +
        "Repeat runs the blocks inside it again and again: type how many times in its box, " +
        "and drop other blocks inside it.\r\n" +
        "\r\n" +
        "Careful: walls stop the robot, and hazards (red tiles) end the run. " +
        "Each level has a limit on how many blocks you can use. Use fewer blocks for a better score!\r\n" +
        "\r\n" +
        "Tip: drag a block back onto the palette to remove it.";

    public HowToPlayForm()
    {
        InitializeComponent();

        txtHelp.Text = HowToPlayText;
        LoadImage();
        ActiveControl = btnClose;
        txtHelp.TabStop = false;
        Shown += (sender, e) => txtHelp.Select(0, 0);
    }

    // Loads the picture. A missing or damaged file is not an error worth stopping for:
    // the picture is hidden and the text gets the whole window.
    private void LoadImage()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Assets",ImageFileName);

        try
        {
            // Copy into a Bitmap so the file isn't left locked while the window is open
            using var fromFile = Image.FromFile(path);
            picHelp.Image = new Bitmap(fromFile);
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException
                                   || ex is OutOfMemoryException || ex is ArgumentException)
        {
            // OutOfMemoryException is what GDI+ throws for a file that isn't a valid image
            picHelp.Visible = false;
            txtHelp.Top = picHelp.Top;
            txtHelp.Height = btnClose.Top - 10 - txtHelp.Top;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            picHelp.Image?.Dispose();   // the bitmap made in LoadImage
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void btnClose_Click(object? sender, EventArgs e) => Close();
}
