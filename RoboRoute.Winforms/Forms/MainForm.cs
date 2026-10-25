using RoboRoute.Winforms.Core;
using RoboRoute.Winforms.Screens;
   // System.Windows.Forms also has a MainMenu

// NOTE: this namespace must match the one at the top of MainForm.Designer.cs.
namespace RoboRoute.Winforms;

// The window. It owns one screen at a time (main menu, level select or the game) and decides what
// happens when a screen raises an event. All the "go to screen X" logic lives here, in one place.
public partial class MainForm : Form
{
    private readonly Panel _host = new Panel();     // the current screen sits in here
    private readonly MainMenuScreen _menu = new MainMenuScreen();
    private readonly LevelSelect _levelSelect = new LevelSelect();
    private readonly GameScreen _game = new GameScreen();

    // The level being played. _currentIndex is its place in Levels.All, or -1 for an imported level.
    private LevelData? _currentLevel;
    private int _currentIndex = -1;

    // Problems found while loading the built-in levels, shown once the window is up
    private string? _startupMessage;

    public MainForm()
    {
        InitializeComponent();

        Text = "RoboRoute";
        ClientSize = new Size(1280, 720);
        MinimumSize = new Size(900, 500);
        StartPosition = FormStartPosition.CenterScreen;

        _host.Dock = DockStyle.Fill;
        Controls.Add(_host);

        LoadBuiltInLevels();

        // ---- Wiring: which event leads where ----
        _menu.PlayClicked += () => ShowScreen(_levelSelect);
        _menu.QuitClicked += Close;

        _levelSelect.LevelChosen += index => StartLevel(Levels.All[index], index);
        _levelSelect.LevelFileChosen += ImportLevel;
        _levelSelect.BackClicked += () => ShowScreen(_menu);

        _game.MenuRequested += () => ShowScreen(_menu);
        _game.LevelCompleted += OnLevelCompleted;

        _levelSelect.SetLevels(Levels.All.Select(level => level.Name).ToList());

        ShowScreen(_menu);

        Shown += (sender, e) =>
        {
            if (_startupMessage != null)
                MessageBox.Show(this, _startupMessage, "Level problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        };
    }

    // ------------------------------------------------------------------
    //  Loading levels
    // ------------------------------------------------------------------

    // Reads Levels/levels.txt. Whatever goes wrong, the game still opens (with fewer levels or none),
    // and the player is told what happened.
    private void LoadBuiltInLevels()
    {
        try
        {
            List<string> skipped = Levels.LoadBuiltIn();

            if (skipped.Count > 0)
                _startupMessage = "Some built-in levels were skipped:\n\n" + string.Join("\n\n", skipped);
        }
        catch (LevelFormatException ex)
        {
            _startupMessage = "The built-in level file has a problem:\n\n" + ex.Message;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            // FileNotFoundException and DirectoryNotFoundException are both IOExceptions
            _startupMessage = "The built-in levels couldn't be read:\n\n" + ex.Message +
                              "\n\nYou can still use Import Level on the level select screen.";
        }

        if (Levels.All.Count == 0 && _startupMessage == null)
            _startupMessage = "No built-in levels were found in " + Levels.FilePath + ".";
    }

    // Called when the player picks a file with the Import Level button.
    // Reads the first level in the file and plays it, or explains what is wrong with the file.
    private void ImportLevel(string path)
    {
        try
        {
            LevelData level = LevelParser.LoadFirstLevel(path);
            StartLevel(level, -1);
        }
        catch (LevelFormatException ex)
        {
            MessageBox.Show(this, ex.Message, "Can't load this level", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            MessageBox.Show(this, "The file couldn't be read:\n\n" + ex.Message, "Can't load this level", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ------------------------------------------------------------------
    //  Screens
    // ------------------------------------------------------------------

    // Swaps the visible screen. The game screen's timer only runs while it is the one showing.
    private void ShowScreen(Control screen)
    {
        _game.Pause();

        _host.Controls.Clear();
        screen.Dock = DockStyle.Fill;
        _host.Controls.Add(screen);

        if (screen == _game)
            _game.Resume();
    }

    // index = the level's place in Levels.All, or -1 for an imported level
    private void StartLevel(LevelData level, int index)
    {
        _currentLevel = level;
        _currentIndex = index;

        _game.LoadLevel(level);
        ShowScreen(_game);
    }

    private void OnLevelCompleted()
    {
        // An imported level has no "next": it was only loaded to be played once
        bool hasNext = _currentIndex >= 0 && _currentIndex + 1 < Levels.All.Count;
        int levelNumber = _currentIndex >= 0 ? _currentIndex + 1 : 0;

        using var popup = new LevelDoneForm(levelNumber, hasNext);
        popup.ShowDialog(this);

        switch (popup.Choice)
        {
            case LevelDoneChoice.NextLevel:
                StartLevel(Levels.All[_currentIndex + 1], _currentIndex + 1);
                break;

            case LevelDoneChoice.MainMenu:
                ShowScreen(_menu);
                break;

            default:    // Restart (also what you get if the window is closed some other way)
                StartLevel(_currentLevel!, _currentIndex);
                break;
        }
    }
}