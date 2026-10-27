
using System.ComponentModel;
using System.Diagnostics;
using RoboRoute.Winforms.Core;


namespace RoboRoute.Winforms.Screens;

// thiss file is for the main menu

public partial class MainMenuScreen : UserControl
{
    private const int DesignWidth = 1440;
    private const int DesignHeight = 1024;
    private Image? _backgroundImage;
    private Image? _robotImage;

    public event Action? PlayClicked;
    public event Action? QuitClicked;

    public MainMenuScreen()
    {
        InitializeComponent();

        string backgroundPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "INTERFACE",
            "MainMenuBGImg.png");

        if (File.Exists(backgroundPath))
        {
            _backgroundImage = Image.FromFile(backgroundPath);
            BackgroundImage = _backgroundImage;
        }

        string robotPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "INTERFACE",
            "RobotTitleArt.png");

        if (File.Exists(robotPath))
        {
            _robotImage = Image.FromFile(robotPath);
            picRobot.Image = _robotImage;
        }

        Resize += (_, _) => PositionHitAreas();
        PositionHitAreas();
    }

    private void btnPlay_Click(object? sender, EventArgs e) => PlayClicked?.Invoke();

    private void btnQuit_Click(object? sender, EventArgs e) => QuitClicked?.Invoke();

    private void PositionHitAreas()
    {
        if (ClientSize.Width == 0 || ClientSize.Height == 0)
            return;

        btnPlay.SetBounds(
            ClientSize.Width * 485 / DesignWidth,
            ClientSize.Height * 760 / DesignHeight,
            ClientSize.Width * 470 / DesignWidth,
            ClientSize.Height * 165 / DesignHeight);

        picRobot.SetBounds(
            ClientSize.Width * 425 / DesignWidth,
            ClientSize.Height * 305 / DesignHeight,
            ClientSize.Width * 590 / DesignWidth,
            ClientSize.Height * 350 / DesignHeight);

    }

    
}
