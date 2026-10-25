namespace RoboRoute.Winforms.Screens;

// The main menu. The look is designed in the Visual Studio designer (MainMenu.Designer.cs);
// this file only turns button clicks into events for MainForm.
//
// To redesign it: double-click MainMenu.cs in Solution Explorer. Keep the buttons named
// btnPlay and btnQuit (or change the two click handlers below to match your names).
public partial class MainMenuScreen : UserControl
{
    public event Action? PlayClicked;
    public event Action? QuitClicked;

    public MainMenuScreen()
    {
        InitializeComponent();
    }

    private void btnPlay_Click(object? sender, EventArgs e) => PlayClicked?.Invoke();

    private void btnQuit_Click(object? sender, EventArgs e) => QuitClicked?.Invoke();
}
