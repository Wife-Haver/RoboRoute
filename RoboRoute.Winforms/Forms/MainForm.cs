
using global::RoboRoute.Winforms.Controls;
using global::RoboRoute.Winforms.Core;
using RoboRoute.Winforms.Controls;
using RoboRoute.Winforms.Core;
using System.Diagnostics;

// NOTE: this namespace must match the one at the top of MainForm.Designer.cs.
// The template default is RoboRoute.Winforms. If your Designer file says something else, change this line to match.
namespace RoboRoute.Winforms;

// The game window: [ level | code queue | block palette ], with draggable dividers between them.
// The layout is built in code (see BuildLayout) so you can read and change all of it in one place.
public partial class MainForm : Form
{
    private static readonly Color PanelColor = Color.FromArgb(30, 30, 40);
    private static readonly Color PaletteColor = Color.FromArgb(45, 35, 55);
    private static readonly Color SplitterColor = Color.FromArgb(80, 80, 100);
    private static readonly Color RunColor = Color.FromArgb(46, 140, 80);
    private static readonly Color StopColor = Color.FromArgb(170, 55, 55);
    private static readonly Color IdleColor = Color.FromArgb(70, 70, 80);

    private static readonly Font HeaderFont = new Font("Segoe UI", 12f, FontStyle.Bold);
    private static readonly Font ButtonFont = new Font("Segoe UI", 12f, FontStyle.Bold);

    // Controls
    private readonly SplitContainer _mainSplit = new SplitContainer();       // level | editor
    private readonly SplitContainer _editorSplit = new SplitContainer();     // queue | palette
    private readonly LevelPanel _levelPanel = new LevelPanel();
    private readonly Panel _queueHost = new Panel();                         // the code queue controls go in here next
    private readonly Panel _paletteHost = new Panel();                       // the block palette controls go in here next
    private readonly Label _blockCounter = new Label();
    private readonly Label _statusLabel = new Label();
    private readonly Button _runButton = new Button();
    private readonly Button _testButton = new Button();

    // Game loop: a timer ticks about 60 times a second, and a stopwatch measures the real time between ticks
    private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
    private readonly Stopwatch _clock = new Stopwatch();

    // Rebuilt for every level (see LoadLevel)
    private int _levelIndex;
    private LevelTileMap _map = null!;
    private Robot _robot = null!;
    private ProgramRunner _runner = null!;
    private bool _levelCompleteShown;

    public MainForm()
    {
        InitializeComponent();   // the designer-generated part, in MainForm.Designer.cs

        Text = "RoboRoute";
        ClientSize = new Size(1280, 720);
        MinimumSize = new Size(900, 500);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = PanelColor;

        BuildLayout();
        LoadLevel(0);

        _timer.Interval = 16;
        _timer.Tick += OnTick;

        Load += OnFormLoad;
    }

    // ------------------------------------------------------------------
    //  Layout
    // ------------------------------------------------------------------

    private void BuildLayout()
    {
        SuspendLayout();

        // Left: the level. Right: the editor (queue + palette). The splitter bar between them can be dragged.
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

    // Middle panel: header with the block counter, the queue area, the status line and the buttons
    private Control BuildQueuePanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(10),
            BackColor = PanelColor
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));          // header
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));     // queue area
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));     // status line
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));     // temporary test button
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));     // Run / Stop button

        // Header: "Code Queue" on the left, "Blocks 0/8" on the right
        Label title = MakeLabel("Code Queue", HeaderFont);
        title.Anchor = AnchorStyles.Left;
        _blockCounter.AutoSize = true;
        _blockCounter.ForeColor = Color.White;
        _blockCounter.Anchor = AnchorStyles.Right;
        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(_blockCounter, 1, 0);

        // Queue area (placeholder until the queue control is written)
        _queueHost.Dock = DockStyle.Fill;
        _queueHost.BackColor = Color.FromArgb(24, 24, 32);
        _queueHost.Margin = new Padding(0, 6, 0, 6);
        _queueHost.Controls.Add(MakePlaceholder("The code queue goes here"));
        layout.Controls.Add(_queueHost, 0, 1);
        layout.SetColumnSpan(_queueHost, 2);

        // Status line ("Running...", "Level complete!", ...)
        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.AutoSize = false;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.ForeColor = Color.White;
        layout.Controls.Add(_statusLabel, 0, 2);
        layout.SetColumnSpan(_statusLabel, 2);

        // TEMPORARY: fills the queue with this level's solution so you can test before drag-and-drop exists
        _testButton.Text = "TEMP: fill in this level's solution";
        _testButton.Dock = DockStyle.Fill;
        _testButton.Margin = new Padding(0, 0, 0, 4);
        StyleButton(_testButton, IdleColor);
        _testButton.Click += OnTestFillClicked;
        layout.Controls.Add(_testButton, 0, 3);
        layout.SetColumnSpan(_testButton, 2);

        // Run / Stop
        _runButton.Dock = DockStyle.Fill;
        _runButton.Margin = new Padding(0);
        _runButton.Font = ButtonFont;
        StyleButton(_runButton, IdleColor);
        _runButton.Click += OnRunClicked;
        layout.Controls.Add(_runButton, 0, 4);
        layout.SetColumnSpan(_runButton, 2);

        return layout;
    }

    // Right panel: header and the area where the draggable blocks will go
    private Control BuildPalettePanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10),
            BackColor = PaletteColor
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        layout.Controls.Add(MakeLabel("Blocks", HeaderFont), 0, 0);

        _paletteHost.Dock = DockStyle.Fill;
        _paletteHost.BackColor = Color.FromArgb(36, 28, 44);
        _paletteHost.Margin = new Padding(0, 6, 0, 0);
        _paletteHost.Controls.Add(MakePlaceholder("The block palette goes here"));
        layout.Controls.Add(_paletteHost, 0, 1);

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

    private static Label MakePlaceholder(string text)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(130, 130, 150)
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

    // ------------------------------------------------------------------
    //  Startup
    // ------------------------------------------------------------------

    private void OnFormLoad(object? sender, EventArgs e)
    {
        // Split positions can only be set once the form has its real size
        PerformLayout();

        _mainSplit.Panel1MinSize = 300;
        _mainSplit.Panel2MinSize = 400;
        CenterSplit(_mainSplit);

        _editorSplit.Panel1MinSize = 150;
        _editorSplit.Panel2MinSize = 150;
        CenterSplit(_editorSplit);

        // Images are looked for in an "Assets" folder next to the .exe
        _levelPanel.LoadAssets(Path.Combine(AppContext.BaseDirectory, "Assets"));
        if (_levelPanel.MissingAssets.Count > 0)
        {
            MessageBox.Show(
                this,
                "These images weren't found in the Assets folder, so placeholders are drawn instead:\n\n  " +
                string.Join("\n  ", _levelPanel.MissingAssets) +
                "\n\nAdd the files and set each one's \"Copy to Output Directory\" to \"Copy if newer\".",
                "Missing images",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        _clock.Restart();
        _timer.Start();
    }

    // Puts a splitter bar in the middle, clamped so it never breaks the panels' minimum sizes
    private static void CenterSplit(SplitContainer split)
    {
        int available = split.Orientation == Orientation.Vertical ? split.Width : split.Height;
        int distance = available / 2;
        distance = Math.Max(split.Panel1MinSize, Math.Min(distance, available - split.Panel2MinSize - split.SplitterWidth));

        if (distance > 0)
            split.SplitterDistance = distance;
    }

    // Builds everything that belongs to one level. Also used to restart a level.
    private void LoadLevel(int index)
    {
        _levelIndex = index;
        LevelData level = Levels.All[index];

        _map = new LevelTileMap(level.Map);
        _robot = new Robot(level.RobotStart);
        _runner = new ProgramRunner(_robot, _map.IsWalkable, _map.IsGoal, _map.IsHazard, level.RobotStart);

        _levelPanel.SetLevel(_map, _robot);
        _levelCompleteShown = false;

        Text = "RoboRoute - Level " + (index + 1) + ": " + level.Name;
        RefreshStatus();
    }

    // ------------------------------------------------------------------
    //  Game loop
    // ------------------------------------------------------------------

    private void OnTick(object? sender, EventArgs e)
    {
        // Real time since the last tick; capped so a long pause (dragging the window, say) doesn't make the robot jump
        float dt = (float)Math.Min(_clock.Elapsed.TotalSeconds, 0.1);
        _clock.Restart();

        _robot.Update(dt);
        _runner.Update();

        RefreshStatus();
        _levelPanel.Invalidate();

        // The program ended on the goal: show the "Level Done" window (once per win)
        if (!_runner.ReachedGoal)
        {
            _levelCompleteShown = false;
        }
        else if (!_levelCompleteShown)
        {
            _levelCompleteShown = true;
            ShowLevelComplete();
        }
    }

    // Copies the runner's state onto the labels and the Run button
    private void RefreshStatus()
    {
        _statusLabel.Text = _runner.Status;
        _statusLabel.ForeColor = _runner.StatusColor;

        int used = 0;
        foreach (Block block in _runner.Queue)
            used += block.TotalBlocks();

        int max = Levels.All[_levelIndex].MaxBlocksAllowed;
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

    // TEMPORARY simple version: replace with a proper Level Done form (Restart / Main Menu / Next Level)
    private void ShowLevelComplete()
    {
        _timer.Stop();

        bool hasNext = _levelIndex + 1 < Levels.All.Length;
        string message = hasNext
            ? "Level done!\n\nGo on to the next level?\n(No restarts this level.)"
            : "Level done! That was the last level.\n\nThe level will restart.";

        DialogResult answer = MessageBox.Show(
            this,
            message,
            "Level Done",
            hasNext ? MessageBoxButtons.YesNo : MessageBoxButtons.OK);

        LoadLevel(hasNext && answer == DialogResult.Yes ? _levelIndex + 1 : _levelIndex);

        _clock.Restart();
        _timer.Start();
    }

    // ------------------------------------------------------------------
    //  Buttons
    // ------------------------------------------------------------------

    private void OnRunClicked(object? sender, EventArgs e)
    {
        if (_runner.IsRunning)
            _runner.Stop();
        else
            _runner.Start();    // does nothing if the queue is empty
    }

    // TEMPORARY: delete this button and method once the queue and palette controls exist
    private void OnTestFillClicked(object? sender, EventArgs e)
    {
        if (_runner.IsRunning) return;

        BlockType move = BlockType.MoveForward;
        BlockType left = BlockType.TurnLeft;
        BlockType right = BlockType.TurnRight;

        List<Block> queue = _runner.Queue;
        queue.Clear();

        switch (_levelIndex)
        {
            case 0:     // First Steps
                AddBlocks(queue, move, move, right, move, move, move, move);
                break;

            case 1:     // Staircase: Repeat 4 [ Move, Turn Right, Move, Turn Left ]
                var loop = new Block(BlockType.Repeat) { Count = 4 };
                AddBlocks(loop.Children, move, right, move, left);
                queue.Add(loop);
                break;

            case 2:     // Hazardous: the detour around the hazard column
                AddBlocks(queue, move, right, move, left, move, right, move, move, right, move, move, left, move);
                break;
        }

        _runner.ClearStatus();
    }

    private static void AddBlocks(List<Block> list, params BlockType[] types)
    {
        foreach (BlockType type in types)
            list.Add(new Block(type));
    }
}
