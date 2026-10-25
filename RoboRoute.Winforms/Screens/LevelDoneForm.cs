namespace RoboRoute.Winforms.Screens;

public enum LevelDoneChoice
{
    Restart,
    MainMenu,
    NextLevel
}

// The "Level done" popup. The look is designed in the Visual Studio designer (LevelDoneForm.Designer.cs).
// MainForm shows it with ShowDialog and then reads Choice to see which button was clicked.
//
// To redesign it: keep the buttons named btnRestart, btnMenu and btnNext, and the label lblTitle
// (or change the handlers below to match your names).
public partial class LevelDoneForm : Form
{
    public LevelDoneChoice Choice { get; private set; } = LevelDoneChoice.Restart;

    public LevelDoneForm(int levelNumber, bool hasNextLevel)
    {
        InitializeComponent();

        lblTitle.Text = levelNumber > 0 ? "Level " + levelNumber + " done!" : "Level done!";
        btnNext.Enabled = hasNextLevel;     // nothing to go to after the last level

        AcceptButton = hasNextLevel ? btnNext : btnMenu;
    }

    private void btnRestart_Click(object? sender, EventArgs e) => Finish(LevelDoneChoice.Restart);

    private void btnMenu_Click(object? sender, EventArgs e) => Finish(LevelDoneChoice.MainMenu);

    private void btnNext_Click(object? sender, EventArgs e) => Finish(LevelDoneChoice.NextLevel);

    private void Finish(LevelDoneChoice choice)
    {
        Choice = choice;
        DialogResult = DialogResult.OK;     // closes the window
    }
    public void ShowResult(int blocksUsed, int best, bool isNewBest)
    {
        lblResult.Text = isNewBest
            ? "Blocks used: " + blocksUsed + "  (new best!)"
            : "Blocks used: " + blocksUsed + "   Best: " + best;
    }
}
