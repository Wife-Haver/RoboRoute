using RoboRoute.Winforms.Core;
using RoboRoute.Winforms.Screens;

// NOTE: this namespace must match the one at the top of MainForm.Designer.cs.
namespace RoboRoute.Winforms;

// The window. It owns one screen at a time (main menu, level select or the game) and decides what
// happens when a screen raises an event. All the "go to screen X" logic lives here, in one place.
public partial class MainForm : Form
{
    private readonly Panel _host = new Panel();     // the current screen sits in here
    //private readonly MainMenuScreen _menu = new MainMenuScreen();
    private readonly LevelSelect _levelSelect = new LevelSelect();
    private readonly GameScreen _game = new GameScreen();

    public MainForm()
    {
        InitializeComponent();

        Text = "RoboRoute";
        ClientSize = new Size(1280, 720);
        MinimumSize = new Size(900, 500);
        StartPosition = FormStartPosition.CenterScreen;

        _host.Dock = DockStyle.Fill;
        Controls.Add(_host);

        // ---- Wiring: which event leads where ----
        //_menu.PlayClicked += () => ShowScreen(_levelSelect);
        //_menu.QuitClicked += Close;

        _levelSelect.LevelChosen += StartLevel;
        //_levelSelect.BackClicked += () => ShowScreen(_menu);

        _game.MenuRequested += ShowMenuFromGame;
        _game.LevelCompleted += OnLevelCompleted;

        _levelSelect.SetLevels(Levels.All.Select(level => level.Name).ToList());

        //ShowScreen(_menu);
    }

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

    private void StartLevel(int index)
    {
        _game.LoadLevel(index);
        ShowScreen(_game);
    }

    //private void ShowMenuFromGame()
    //{
    //    ShowScreen(_menu);
    //}

    private void OnLevelCompleted(int levelIndex)
    {
        bool hasNext = levelIndex + 1 < Levels.All.Length;

        using var popup = new LevelDoneForm(levelIndex + 1, hasNext);
        popup.ShowDialog(this);

        switch (popup.Choice)
        {
            case LevelDoneChoice.NextLevel:
                StartLevel(levelIndex + 1);
                break;

            //case LevelDoneChoice.MainMenu:
            //    ShowScreen(_menu);
            //    break;

            default:    // Restart (also what you get if the window is closed some other way)
                StartLevel(levelIndex);
                break;
        }
    }
}