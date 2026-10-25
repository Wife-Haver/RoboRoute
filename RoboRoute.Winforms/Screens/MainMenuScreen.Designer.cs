namespace RoboRoute.Winforms.Screens;

partial class MainMenuScreen
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        lblTitle = new Label();
        btnPlay = new Button();
        btnQuit = new Button();
        SuspendLayout();
        //
        // lblTitle
        //
        lblTitle.Anchor = AnchorStyles.Top;
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(420, 120);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(260, 65);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "RoboRoute";
        //
        // btnPlay
        //
        btnPlay.Anchor = AnchorStyles.Top;
        btnPlay.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        btnPlay.Location = new Point(420, 260);
        btnPlay.Name = "btnPlay";
        btnPlay.Size = new Size(260, 60);
        btnPlay.TabIndex = 1;
        btnPlay.Text = "Play";
        btnPlay.UseVisualStyleBackColor = true;
        btnPlay.Click += btnPlay_Click;
        //
        // btnQuit
        //
        btnQuit.Anchor = AnchorStyles.Top;
        btnQuit.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        btnQuit.Location = new Point(420, 340);
        btnQuit.Name = "btnQuit";
        btnQuit.Size = new Size(260, 60);
        btnQuit.TabIndex = 2;
        btnQuit.Text = "Quit";
        btnQuit.UseVisualStyleBackColor = true;
        btnQuit.Click += btnQuit_Click;
        //
        // MainMenu
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 40);
        Controls.Add(btnQuit);
        Controls.Add(btnPlay);
        Controls.Add(lblTitle);
        Name = "MainMenu";
        Size = new Size(1100, 600);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTitle;
    private Button btnPlay;
    private Button btnQuit;
}
