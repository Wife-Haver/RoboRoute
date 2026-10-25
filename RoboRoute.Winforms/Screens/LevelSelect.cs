namespace RoboRoute.Winforms.Screens;

// The level select screen. The title, the Back button and the panel that holds the level buttons are
// designed in the Visual Studio designer; the level buttons themselves are created in code, one per level,
// so adding a level to Levels.cs adds a button automatically.
//
// To redesign it: keep a container named flowLevels (a FlowLayoutPanel works well) and a button named btnBack.


public partial class LevelSelect : UserControl
{
    // The int is the level's index in Levels.All (0 = first level)
    public event Action<int>? LevelChosen;
    public event Action? BackClicked;

    public LevelSelect()
    {
        InitializeComponent();
    }

    // Makes one button per level name
    public void SetLevels(IReadOnlyList<string> names)
    {
        flowLevels.SuspendLayout();

        foreach (Control old in flowLevels.Controls.Cast<Control>().ToList())
        {
            flowLevels.Controls.Remove(old);
            old.Dispose();
        }

        for (int i = 0; i < names.Count; i++)
        {
            int index = i;      // each button remembers its own level (a loop variable would not work here)

            var button = new Button
            {
                Text = (i + 1) + "\n" + names[i],
                Size = new Size(140, 100),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Margin = new Padding(10)
            };
            button.Click += (sender, e) => LevelChosen?.Invoke(index);
            flowLevels.Controls.Add(button);
        }

        flowLevels.ResumeLayout(true);
    }

    private void btnBack_Click(object? sender, EventArgs e) => BackClicked?.Invoke();

    private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
    {

    }

    public event Action<string>? LevelFileChosen;

}
