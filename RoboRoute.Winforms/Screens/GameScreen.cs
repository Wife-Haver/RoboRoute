using System.Diagnostics;
using RoboRoute.Winforms.Controls;
using RoboRoute.Winforms.Core;

namespace RoboRoute.Winforms.Screens;

// The puzzle screen: [ level | code queue | block palette ].
// It is a UserControl that MainForm swaps in and out. It has no designer file because its layout is built in code.
//
// It talks to the outside through events and four methods:
//   LoadLevel(i)  - set up level i
//   Resume()      - start the game timer (call after the screen is shown)
//   Pause()       - stop the game timer (call before the screen is hidden or a popup opens)
//   LevelCompleted(levelIndex), MenuRequested - what the screen tells MainForm
public class GameScreen : UserControl
{
    private static readonly Color PanelColor = Color.FromArgb(30, 30, 40);
    private static readonly Color PaletteColor = Color.FromArgb(45, 35, 55);
    private static readonly Color SplitterColor = Color.FromArgb(80, 80, 100);
    private static readonly Color RunColor = Color.FromArgb(46, 140, 80);
    private static readonly Color StopColor = Color.FromArgb(170, 55, 55);
    private static readonly Color IdleColor = Color.FromArgb(70, 70, 80);

    private static readonly Font HeaderFont = new Font("Segoe UI", 12f, FontStyle.Bold);
    private static readonly Font ButtonFont = new Font("Segoe UI", 12f, FontStyle.Bold);

    private readonly SplitContainer _mainSplit = new SplitContainer();       // level | editor
    private readonly SplitContainer _editorSplit = new SplitContainer();     // queue | palette
    private readonly LevelPanel _levelPanel = new LevelPanel();
    private readonly Panel _queueHost = new Panel();                         // the CodeEditor builds the queue in here
    private readonly Panel _paletteHost = new Panel();                       // ...and the block palette in here
    private readonly Label _blockCounter = new Label();
    private readonly Label _statusLabel = new Label();
    private readonly Button _runButton = new Button();
    private readonly Button _pauseButton = new Button();


    private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
    private readonly Stopwatch _clock = new Stopwatch();

    private LevelData _level = null!;
    private LevelTileMap _map = null!;
    private Robot _robot = null!;
    private ProgramRunner _runner = null!;
    private CodeEditor? _editor;
    private bool _splitsReady;
    private bool _assetsLoaded;

    // Raised when the program ended on the goal. The int is the index of the level just finished.
    public event Action? LevelCompleted;

    // Raised when the player clicks the Menu button
    public event Action? PauseRequested;

    // Total blocks in the queue, invlcudingblocks inside Repeat blocks
    public int BlocksUsed => _runner.Queue.Sum(block => block.TotalBlocks());



    public GameScreen()
    {
        BackColor = PanelColor;
        BuildLayout();

        _timer.Interval = 16;
        _timer.Tick += OnTick;


    }   

    // ------------------------------------------------------------------
    //  Public API
    // ------------------------------------------------------------------

    // Builds everything that belongs to one level. Also used to restart a level.
    public void LoadLevel(LevelData level)
    {
        _level = level;

        _map = new LevelTileMap(level.Map);
        _robot = new Robot(level.RobotStart, level.RobotFacing);
        _runner = new ProgramRunner(_robot, _map.IsWalkable, _map.IsGoal, _map.IsHazard, level.RobotStart, level.RobotFacing);

        _levelPanel.SetLevel(_map, _robot);

        _editor?.Dispose();
        _editor = new CodeEditor(_runner, level, _queueHost, _paletteHost);

        RefreshStatus();
    }

    // Starts the game loop. Call this after the screen has been added to the form.
    public void Resume()
    {
        if (!_splitsReady && Width > 0)
            SetUpSplitsAndAssets();

        _clock.Restart();
        _timer.Start();
    }

    // Stops the game loop (the robot freezes where it is)
    public void Pause()
    {
        _timer.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            _editor?.Dispose();
        }
        base.Dispose(disposing);
    }

    // ------------------------------------------------------------------
    //  Layout
    // ------------------------------------------------------------------

    private void BuildLayout()
    {
        SuspendLayout();

        _mainSplit.Dock = DockStyle.Fill;
        _mainSplit.Orientation = Orientation.Vertical;      // vertical bar = panels side by side
        _mainSplit.SplitterWidth = 6;
        _mainSplit.BackColor = SplitterColor;

        _levelPanel.Dock = DockStyle.Fill;
        _mainSplit.Panel1.Controls.Add(_levelPanel);

        _editorSplit.Dock = DockStyle.Fill;
        _editorSplit.Orientation = Orientation.Vertical;
        _editorSplit.SplitterWidth = 6;
        _editorSplit.BackColor = SplitterColor;
        _editorSplit.Panel1.BackColor = PanelColor;
        _editorSplit.Panel2.BackColor = PaletteColor;
        _editorSplit.Panel1.Controls.Add(BuildQueuePanel());
        _editorSplit.Panel2.Controls.Add(BuildPalettePanel());
        _mainSplit.Panel2.Controls.Add(_editorSplit);

        Controls.Add(_mainSplit);

        ResumeLayout(true);
    }

    private Control BuildQueuePanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(10),
            BackColor = PanelColor
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // header
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));     // queue area
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));     // status line
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));     // Run / Stop button

        Label title = MakeLabel("Code Queue", HeaderFont);
        title.Anchor = AnchorStyles.Left;
        _blockCounter.AutoSize = true;
        _blockCounter.ForeColor = Color.White;
        _blockCounter.Anchor = AnchorStyles.Right;
        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(_blockCounter, 1, 0);

        _queueHost.Dock = DockStyle.Fill;
        _queueHost.BackColor = UiStyle.QueueBack;
        _queueHost.Margin = new Padding(0, 6, 0, 6);
        layout.Controls.Add(_queueHost, 0, 1);
        layout.SetColumnSpan(_queueHost, 2);

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.AutoSize = false;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.ForeColor = Color.White;
        layout.Controls.Add(_statusLabel, 0, 2);
        layout.SetColumnSpan(_statusLabel, 2);

        _runButton.Dock = DockStyle.Fill;
        _runButton.Margin = new Padding(0);
        _runButton.Font = ButtonFont;
        StyleButton(_runButton, IdleColor);
        _runButton.Click += OnRunClicked;
        layout.Controls.Add(_runButton, 0, 3);
        layout.SetColumnSpan(_runButton, 2);

        return layout;
    }

    private Control BuildPalettePanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2    ,
            Padding = new Padding(10),
            BackColor = PaletteColor
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));   // pause button, last row


        layout.Controls.Add(MakeLabel("Blocks", HeaderFont), 0, 0);

        _paletteHost.Dock = DockStyle.Fill;
        _paletteHost.BackColor = Color.FromArgb(36, 28, 44);
        _paletteHost.Margin = new Padding(0, 6, 0, 6);
        
        layout.Controls.Add(_paletteHost, 0, 1);

        _pauseButton.Text = "PAUSE";
        _pauseButton.Dock = DockStyle.Fill;
        _pauseButton.Margin = new Padding(0);
        _pauseButton.Font = ButtonFont;
        StyleButton(_pauseButton, IdleColor);
        _pauseButton.Click += (sender, e) => PauseRequested?.Invoke();
        layout.Controls.Add(_pauseButton, 0, 2);


        return layout;
    }

    private static Label MakeLabel(string text, Font font)
    {
        return new Label
        {
            Text = text,
            Font = font,
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6)
        };
    }

    private static void StyleButton(Button button, Color color)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Color.White;
        button.FlatAppearance.BorderSize = 2;
        button.BackColor = color;
        button.ForeColor = Color.White;
        button.UseVisualStyleBackColor = false;
    }

    // Split positions and images can only be set once the screen has its real size, so this runs on the first Resume()
    private void SetUpSplitsAndAssets()
    {
        _splitsReady = true;
        PerformLayout();

        _mainSplit.Panel1MinSize = 300;
        _mainSplit.Panel2MinSize = 400;
        CenterSplit(_mainSplit);

        _editorSplit.Panel1MinSize = 150;
        _editorSplit.Panel2MinSize = 150;
        CenterSplit(_editorSplit);

        if (_assetsLoaded) return;
        _assetsLoaded = true;

        _levelPanel.LoadAssets(Path.Combine(AppContext.BaseDirectory, "Assets"));
        if (_levelPanel.MissingAssets.Count > 0)
        {
            MessageBox.Show(
                FindForm(),
                "These images weren't found in the Assets folder, so placeholders are drawn instead:\n\n  " +
                string.Join("\n  ", _levelPanel.MissingAssets) +
                "\n\nAdd the files and set each one's \"Copy to Output Directory\" to \"Copy if newer\".",
                "Missing images",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    // Puts a splitter bar in the middle, clamped so it never breaks the panels' minimum sizes
    private static void CenterSplit(SplitContainer split)
    {
        int available = split.Width;
        int distance = available / 2;
        distance = Math.Max(split.Panel1MinSize, Math.Min(distance, available - split.Panel2MinSize - split.SplitterWidth));

        if (distance > 0)
            split.SplitterDistance = distance;
    }

    // ------------------------------------------------------------------
    //  Game loop
    // ------------------------------------------------------------------

    private void OnTick(object? sender, EventArgs e)
    {
        // Real time since the last tick; capped so a long pause (dragging the window) doesn't make the robot jump
        float dt = (float)Math.Min(_clock.Elapsed.TotalSeconds, 0.1);
        _clock.Restart();

        _robot.Update(dt);
        _runner.Update();
        _editor?.Update();

        RefreshStatus();
        _levelPanel.Invalidate();

        if (_runner.ReachedGoal)
        {
            // Stop first: the popup the listener opens runs its own message loop
            Pause();
            LevelCompleted?.Invoke();
        }
    }

    private void RefreshStatus()
    {
        _statusLabel.Text = _runner.Status;
        _statusLabel.ForeColor = _runner.StatusColor;

        int used = 0;
        foreach (Block block in _runner.Queue)
            used += block.TotalBlocks();

        int max = _level.MaxBlocksAllowed;
        _blockCounter.Text = "Blocks " + used + "/" + max;
        _blockCounter.ForeColor = used >= max ? Color.FromArgb(255, 200, 100) : Color.White;

        if (_runner.IsRunning)
        {
            _runButton.Text = "STOP";
            _runButton.BackColor = StopColor;
        }
        else
        {
            _runButton.Text = "RUN";
            _runButton.BackColor = _runner.Queue.Count == 0 ? IdleColor : RunColor;
        }
    }

    private void OnRunClicked(object? sender, EventArgs e)
    {
        if (_runner.IsRunning)
            _runner.Stop();
        else
            _runner.Start();    // does nothing if the queue is empty
    }
}
