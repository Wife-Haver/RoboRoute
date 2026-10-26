namespace RoboRoute.Winforms.Screens;

partial class PauseForm
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

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblTitle = new Label();
        btnResume = new Button();
        btnRestart = new Button();
        btnLevelSelect = new Button();
        btnMainMenu = new Button();
        SuspendLayout();
        //
        // lblTitle
        //
        lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(20, 15);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(260, 50);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Paused";
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        //
        // btnResume
        //
        btnResume.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        btnResume.Location = new Point(40, 80);
        btnResume.Name = "btnResume";
        btnResume.Size = new Size(220, 50);
        btnResume.TabIndex = 1;
        btnResume.Text = "Resume";
        btnResume.UseVisualStyleBackColor = true;
        btnResume.Click += btnResume_Click;
        //
        // btnRestart
        //
        btnRestart.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        btnRestart.Location = new Point(40, 140);
        btnRestart.Name = "btnRestart";
        btnRestart.Size = new Size(220, 50);
        btnRestart.TabIndex = 2;
        btnRestart.Text = "Restart level";
        btnRestart.UseVisualStyleBackColor = true;
        btnRestart.Click += btnRestart_Click;
        //
        // btnLevelSelect
        //
        btnLevelSelect.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        btnLevelSelect.Location = new Point(40, 200);
        btnLevelSelect.Name = "btnLevelSelect";
        btnLevelSelect.Size = new Size(220, 50);
        btnLevelSelect.TabIndex = 3;
        btnLevelSelect.Text = "Level select";
        btnLevelSelect.UseVisualStyleBackColor = true;
        btnLevelSelect.Click += btnLevelSelect_Click;
        //
        // btnMainMenu
        //
        btnMainMenu.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        btnMainMenu.Location = new Point(40, 260);
        btnMainMenu.Name = "btnMainMenu";
        btnMainMenu.Size = new Size(220, 50);
        btnMainMenu.TabIndex = 4;
        btnMainMenu.Text = "Main menu";
        btnMainMenu.UseVisualStyleBackColor = true;
        btnMainMenu.Click += btnMainMenu_Click;
        //
        // PauseForm
        //
        AcceptButton = btnResume;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 40);
        CancelButton = btnResume;
        ClientSize = new Size(300, 335);
        ControlBox = false;
        Controls.Add(btnMainMenu);
        Controls.Add(btnLevelSelect);
        Controls.Add(btnRestart);
        Controls.Add(btnResume);
        Controls.Add(lblTitle);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "PauseForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Paused";
        ResumeLayout(false);
    }

    #endregion

    private Label lblTitle;
    private Button btnResume;
    private Button btnRestart;
    private Button btnLevelSelect;
    private Button btnMainMenu;
}
