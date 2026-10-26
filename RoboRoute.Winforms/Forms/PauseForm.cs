namespace RoboRoute.Winforms.Screens;

public enum PauseChoice
{
    Resume,
    Restart,
    LevelSelect,
    MainMenu
}

// The pause menu popup. The look is designed in the Visual Studio designer (PauseForm.Designer.cs).
// MainForm shows it with ShowDialog and then reads Choice to see which button was clicked.
//
// Pressing Esc or closing it counts as Resume (CancelButton is set to btnResume).
// To redesign it: keep the buttons named btnResume, btnRestart, btnLevelSelect and btnMainMenu
// (or change the handlers below to match your names).
public partial class PauseForm : Form
{
    public PauseChoice Choice { get; private set; } = PauseChoice.Resume;

    public PauseForm()
    {
        InitializeComponent();
    }

    private void btnResume_Click(object? sender, EventArgs e) => Finish(PauseChoice.Resume);

    private void btnRestart_Click(object? sender, EventArgs e) => Finish(PauseChoice.Restart);

    private void btnLevelSelect_Click(object? sender, EventArgs e) => Finish(PauseChoice.LevelSelect);

    private void btnMainMenu_Click(object? sender, EventArgs e) => Finish(PauseChoice.MainMenu);

    private void Finish(PauseChoice choice)
    {
        Choice = choice;
        DialogResult = DialogResult.OK;     // closes the window
    }
}
