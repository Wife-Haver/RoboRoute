namespace RoboRoute.Winforms.Screens;

// The level select screen. The title, the Back button and the panel that holds the level buttons are
// designed in the Visual Studio designer; the level buttons themselves are created in code, one per level,
// so adding a level to Levels.cs adds a button automatically.
//
// To redesign it: keep a container named flowLevels (a FlowLayoutPanel works well) and a button named btnBack.


public partial class LevelSelect : UserControl
{

    public event Action<int>? LevelChosen;
    public event Action? BackClicked;

    public LevelSelect()
    {
        InitializeComponent();
    }

    // Makes one button per level name
    public record LevelEntry(string Name, bool Unlocked, int? BestBlocks);

    public void SetLevels(IReadOnlyList<LevelEntry> levels)
    {
        flowLevels.SuspendLayout();

        foreach (Control old in flowLevels.Controls.Cast<Control>().ToList())
        {
            flowLevels.Controls.Remove(old);
            old.Dispose();
        }

        for (int i = 0; i < levels.Count; i++)
        {
            int index = i;
            LevelEntry entry = levels[i];

            string status = !entry.Unlocked ? "Locked"
                          : entry.BestBlocks is int best ? "Best: " + best
                          : "Not done yet";

            var button = new Button
            {
                Text = (i + 1) + "\n" + entry.Name + "\n" + status,
                Size = new Size(140, 100),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Margin = new Padding(10),
                Enabled = entry.Unlocked
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

    private void lblTitle_Click(object sender, EventArgs e)
    {

    }

    public event Action<string>? LevelFileChosen;

}
