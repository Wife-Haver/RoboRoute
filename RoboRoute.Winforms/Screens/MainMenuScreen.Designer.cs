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
        btnShowSave = new Button();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.Anchor = AnchorStyles.Top;
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(480, 160);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(350, 81);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "RoboRoute";
        // 
        // btnPlay
        // 
        btnPlay.Anchor = AnchorStyles.Top;
        btnPlay.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        btnPlay.Location = new Point(480, 347);
        btnPlay.Margin = new Padding(3, 4, 3, 4);
        btnPlay.Name = "btnPlay";
        btnPlay.Size = new Size(297, 80);
        btnPlay.TabIndex = 1;
        btnPlay.Text = "Play";
        btnPlay.UseVisualStyleBackColor = true;
        btnPlay.Click += btnPlay_Click;
        // 
        // btnQuit
        // 
        btnQuit.Anchor = AnchorStyles.Top;
        btnQuit.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        btnQuit.Location = new Point(480, 453);
        btnQuit.Margin = new Padding(3, 4, 3, 4);
        btnQuit.Name = "btnQuit";
        btnQuit.Size = new Size(297, 80);
        btnQuit.TabIndex = 2;
        btnQuit.Text = "Quit";
        btnQuit.UseVisualStyleBackColor = true;
        btnQuit.Click += btnQuit_Click;
        // 
        // btnShowSave
        // 
        btnShowSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnShowSave.Location = new Point(16, 757);
        btnShowSave.Name = "btnShowSave";
        btnShowSave.Size = new Size(119, 29);
        btnShowSave.TabIndex = 3;
        btnShowSave.Text = "Show Save FIle";
        btnShowSave.UseVisualStyleBackColor = true;
        btnShowSave.Click += btnShowSave_Click;
        // 
        // MainMenuScreen
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 40);
        Controls.Add(btnShowSave);
        Controls.Add(btnQuit);
        Controls.Add(btnPlay);
        Controls.Add(lblTitle);
        Margin = new Padding(3, 4, 3, 4);
        Name = "MainMenuScreen";
        Size = new Size(1257, 800);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTitle;
    private Button btnPlay;
    private Button btnQuit;
    private Button btnShowSave;
}
