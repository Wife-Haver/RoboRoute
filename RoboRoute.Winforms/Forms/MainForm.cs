using RoboRoute.Winforms.Core;
using RoboRoute.Winforms.Screens;
using static RoboRoute.Winforms.Screens.LevelSelect;
 

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
    private readonly PauseScreen _pause = new PauseScreen();

    // Which levels are beaten, and the best score for each (loaded from, and saved to, a file)
    private readonly SaveData _save = SaveData.Load();

    // The level being played, and its place in Levels.All
    private LevelData? _currentLevel;
    private int _currentIndex;

    // Problems found while loading levels or the save file, shown once the window is up
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

        if (_save.LoadWarning != null)
            _startupMessage = (_startupMessage == null ? "" : _startupMessage + "\n\n") + _save.LoadWarning;

        // ---- Wiring: which event leads where ----
        _menu.PlayClicked += ShowLevelSelect;
        _menu.QuitClicked += Close;

        _levelSelect.LevelChosen += index => StartLevel(Levels.All[index], index);
        _levelSelect.BackClicked += () => ShowScreen(_menu);

        _game.PauseRequested += () => ShowScreen(_pause);
        _game.LevelCompleted += OnLevelCompleted;

        _pause.MainMenuClicked += () => ShowScreen(_menu);
        _pause.RestartLevelClicked += RestartCurrentLevel;
        _pause.SelectLevelClicked += ShowLevelSelect;

        ShowScreen(_menu);

        Shown += (sender, e) =>
        {
            if (_startupMessage != null)
                MessageBox.Show(this, _startupMessage, "Problem loading", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            _startupMessage = "The built-in levels couldn't be read:\n\n" + ex.Message;
        }

        if (Levels.All.Count == 0 && _startupMessage == null)
            _startupMessage = "No built-in levels were found in " + Levels.FilePath + ".";
    }

    // ------------------------------------------------------------------
    //  Progress: locking levels and scores
    // ------------------------------------------------------------------

    // The first level is always open; every other level opens once the one before it has been beaten
    private bool IsUnlocked(int index)
    {
        return index == 0 || _save.IsCompleted(Levels.All[index - 1].Name);
    }

    // Rebuilds the level buttons from the current progress, then shows the level select screen
    private void ShowLevelSelect()
    {
        var entries = new List<LevelEntry>();

        for (int i = 0; i < Levels.All.Count; i++)
        {
            string name = Levels.All[i].Name;
            entries.Add(new LevelEntry(name, IsUnlocked(i), _save.GetBest(name)));
        }

        _levelSelect.SetLevels(entries);
        ShowScreen(_levelSelect);
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

    private void StartLevel(LevelData level, int index)
    {
        _currentLevel = level;
        _currentIndex = index;

        _game.LoadLevel(level);
        ShowScreen(_game);
    }

    private void RestartCurrentLevel()
    {
        if (_currentLevel != null)
            StartLevel(_currentLevel, _currentIndex);
    }

    private void OnLevelCompleted()
    {
        LevelData level = _currentLevel!;
        int blocksUsed = _game.BlocksUsed;

        // Save the result first, so the next level is already unlocked when the popup closes
        bool isNewBest = _save.Record(level.Name, blocksUsed);
        if (isNewBest)
        {
            string? error = _save.Save();
            if (error != null)
                MessageBox.Show(this, error, "Couldn't save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        bool hasNext = _currentIndex + 1 < Levels.All.Count;
        int best = _save.GetBest(level.Name) ?? blocksUsed;

        using var popup = new LevelDoneForm(_currentIndex + 1, hasNext);
        popup.ShowResult(blocksUsed, best, isNewBest);
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
                StartLevel(level, _currentIndex);
                break;
        }
    }
}