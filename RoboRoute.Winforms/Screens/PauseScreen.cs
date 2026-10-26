namespace RoboRoute.Winforms.Screens;

public sealed class PauseScreen : UserControl
{
    private const int DesignWidth = 1440;
    private const int DesignHeight = 1024;

    private Image? _backgroundImage;
    private readonly List<Image> _buttonImages = new();

    private readonly Button _mainMenuButton = CreateImageButton("MAIN MENU BUTTON.png");
    private readonly Button _restartButton = CreateImageButton("Restart Level Button.png");
    private readonly Button _selectLevelButton = CreateImageButton("SELECT LEVEL.png");

    public event Action? MainMenuClicked;
    public event Action? RestartLevelClicked;
    public event Action? SelectLevelClicked;

    public PauseScreen()
    {
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Color.FromArgb(225, 235, 245);
        BackgroundImageLayout = ImageLayout.Stretch;

        LoadImageAssets();

        _mainMenuButton.Click += (_, _) => MainMenuClicked?.Invoke();
        _restartButton.Click += (_, _) => RestartLevelClicked?.Invoke();
        _selectLevelButton.Click += (_, _) => SelectLevelClicked?.Invoke();

        Controls.Add(_selectLevelButton);
        Controls.Add(_restartButton);
        Controls.Add(_mainMenuButton);

        Resize += (_, _) => PositionButtons();
        PositionButtons();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _backgroundImage?.Dispose();
            foreach (Image image in _buttonImages)
                image.Dispose();
        }

        base.Dispose(disposing);
    }

    private void LoadImageAssets()
    {
        string assetDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "INTERFACE");

        _backgroundImage = LoadImage(assetDirectory, "Desktop - 2 (1).png");
        if (_backgroundImage != null)
            BackgroundImage = _backgroundImage;

        LoadButtonImage(_mainMenuButton, assetDirectory, "MAIN MENU BUTTON.png");
        LoadButtonImage(_restartButton, assetDirectory, "Restart Level Button.png");
        LoadButtonImage(_selectLevelButton, assetDirectory, "SELECT LEVEL.png");
    }

    private void LoadButtonImage(Button button, string directory, string fileName)
    {
        Image? image = LoadImage(directory, fileName);
        if (image == null)
            return;

        _buttonImages.Add(image);
        button.BackgroundImage = image;
        button.BackgroundImageLayout = ImageLayout.Stretch;
    }

    private static Image? LoadImage(string directory, string fileName)
    {
        string path = Path.Combine(directory, fileName);
        return File.Exists(path) ? Image.FromFile(path) : null;
    }

    private static Button CreateImageButton(string accessibleName)
    {
        return new Button
        {
            AccessibleName = accessibleName,
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            FlatAppearance =
            {
                BorderSize = 0,
                MouseDownBackColor = Color.Transparent,
                MouseOverBackColor = Color.Transparent
            },
            TabStop = true,
            Text = string.Empty,
            UseVisualStyleBackColor = false
        };
    }

    private void PositionButtons()
    {
        if (ClientSize.Width == 0 || ClientSize.Height == 0)
            return;

        _mainMenuButton.SetBounds(
            ClientSize.Width * 470 / DesignWidth,
            ClientSize.Height * 305 / DesignHeight,
            ClientSize.Width * 500 / DesignWidth,
            ClientSize.Height * 160 / DesignHeight);

        _restartButton.SetBounds(
            ClientSize.Width * 470 / DesignWidth,
            ClientSize.Height * 495 / DesignHeight,
            ClientSize.Width * 500 / DesignWidth,
            ClientSize.Height * 160 / DesignHeight);

        _selectLevelButton.SetBounds(
            ClientSize.Width * 470 / DesignWidth,
            ClientSize.Height * 680 / DesignHeight,
            ClientSize.Width * 500 / DesignWidth,
            ClientSize.Height * 160 / DesignHeight);
    }
}
