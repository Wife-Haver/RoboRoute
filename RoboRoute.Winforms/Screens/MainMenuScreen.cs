
using System.ComponentModel;
using System.Diagnostics;
using RoboRoute.Winforms.Core;


namespace RoboRoute.Winforms.Screens;

// thiss file is for the main menu

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

    private void btnShowSave_Click(object sender, EventArgs e)
    {
        string file = SaveData.FilePath;
        string folder = Path.GetDirectoryName(file)!;

        try
        {
            if (File.Exists(file))
            {
                

                Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + file + "\"") { UseShellExecute = true });
            }
            else if (Directory.Exists(folder))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", "\"" + folder + "\"") { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show(this,
                    "There's no save file yet. It is created the first time you beat a level.\n\nIt will be saved to:\n" + file,
                    "No save file", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex) when (ex is Win32Exception || ex is InvalidOperationException)
        {
            MessageBox.Show(this, "Couldn't open File Explorer:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
