namespace RoboRoute.Winforms.Screens;

partial class MainMenuScreen
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
            _robotImage?.Dispose();
            _backgroundImage?.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        btnPlay = new Button();
        btnQuit = new Button();
        btnShowSave = new Button();
        btnHowToPlay = new Button();
        btnAboutUs = new Button();
        btnSettings = new Button();
        picRobot = new PictureBox();
        ((System.ComponentModel.ISupportInitialize)picRobot).BeginInit();
        SuspendLayout();
        //
        // btnPlay
        //
        btnPlay.BackColor = Color.Transparent;
        btnPlay.FlatAppearance.BorderSize = 0;
        btnPlay.FlatAppearance.MouseDownBackColor = Color.Transparent;
        btnPlay.FlatAppearance.MouseOverBackColor = Color.Transparent;
        btnPlay.FlatStyle = FlatStyle.Flat;
        btnPlay.Name = "btnPlay";
        btnPlay.TabStop = true;
        btnPlay.Click += btnPlay_Click;
        //
        // btnQuit
        //
        btnQuit.Name = "btnQuit";
        btnQuit.Visible = false;
        btnQuit.Click += btnQuit_Click;
        //
        // btnShowSave
        //
        btnShowSave.Name = "btnShowSave";
        btnShowSave.Visible = false;
        btnShowSave.Click += btnShowSave_Click;
        //
        // btnHowToPlay
        //
        ConfigureTransparentButton(btnHowToPlay, "How to Play");
        //
        // btnAboutUs
        //
        ConfigureTransparentButton(btnAboutUs, "About Us");
        //
        // btnSettings
        //
        ConfigureTransparentButton(btnSettings, "Settings");
        //
        // picRobot
        //
        picRobot.BackColor = Color.Transparent;
        picRobot.Name = "picRobot";
        picRobot.SizeMode = PictureBoxSizeMode.Zoom;
        picRobot.TabStop = false;
        //
        // MainMenuScreen
        // 
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Color.FromArgb(225, 235, 245);
        BackgroundImageLayout = ImageLayout.Stretch;
        Controls.Add(btnShowSave);
        Controls.Add(btnQuit);
        Controls.Add(btnSettings);
        Controls.Add(btnAboutUs);
        Controls.Add(btnHowToPlay);
        Controls.Add(picRobot);
        Controls.Add(btnPlay);
        Name = "MainMenuScreen";
        Size = new Size(1440, 1024);
        ((System.ComponentModel.ISupportInitialize)picRobot).EndInit();
        ResumeLayout(false);
    }

    private static void ConfigureTransparentButton(Button button, string accessibleName)
    {
        button.AccessibleName = accessibleName;
        button.BackColor = Color.Transparent;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = Color.Transparent;
        button.FlatAppearance.MouseOverBackColor = Color.Transparent;
        button.FlatStyle = FlatStyle.Flat;
        button.TabStop = true;
    }

    #endregion

    private Button btnPlay;
    private Button btnQuit;
    private Button btnShowSave;
    private Button btnHowToPlay;
    private Button btnAboutUs;
    private Button btnSettings;
    private PictureBox picRobot;
}
