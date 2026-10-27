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
        btnPlay.Location = new Point(0, 0);
        btnPlay.Name = "btnPlay";
        btnPlay.Size = new Size(75, 23);
        btnPlay.TabIndex = 3;
        btnPlay.UseVisualStyleBackColor = false;
        btnPlay.Click += btnPlay_Click;
        // 
        // picRobot
        // 
        picRobot.BackColor = Color.Transparent;
        picRobot.Location = new Point(0, 0);
        picRobot.Name = "picRobot";
        picRobot.Size = new Size(100, 50);
        picRobot.SizeMode = PictureBoxSizeMode.Zoom;
        picRobot.TabIndex = 2;
        picRobot.TabStop = false;
        // 
        // MainMenuScreen
        // 
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Color.FromArgb(225, 235, 245);
        BackgroundImageLayout = ImageLayout.Stretch;
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


    private PictureBox picRobot;
}
