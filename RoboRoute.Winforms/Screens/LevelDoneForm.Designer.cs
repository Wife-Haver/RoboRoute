namespace RoboRoute.Winforms.Screens;

partial class LevelDoneForm
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
        btnRestart = new Button();
        btnMenu = new Button();
        btnNext = new Button();
        SuspendLayout();
        //
        // lblTitle
        //
        lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(20, 20);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(380, 60);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Level done!";
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        //
        // btnRestart
        //
        btnRestart.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnRestart.Location = new Point(20, 110);
        btnRestart.Name = "btnRestart";
        btnRestart.Size = new Size(115, 50);
        btnRestart.TabIndex = 1;
        btnRestart.Text = "Restart";
        btnRestart.UseVisualStyleBackColor = true;
        btnRestart.Click += btnRestart_Click;
        //
        // btnMenu
        //
        btnMenu.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnMenu.Location = new Point(152, 110);
        btnMenu.Name = "btnMenu";
        btnMenu.Size = new Size(115, 50);
        btnMenu.TabIndex = 2;
        btnMenu.Text = "Main menu";
        btnMenu.UseVisualStyleBackColor = true;
        btnMenu.Click += btnMenu_Click;
        //
        // btnNext
        //
        btnNext.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnNext.Location = new Point(285, 110);
        btnNext.Name = "btnNext";
        btnNext.Size = new Size(115, 50);
        btnNext.TabIndex = 3;
        btnNext.Text = "Next level";
        btnNext.UseVisualStyleBackColor = true;
        btnNext.Click += btnNext_Click;
        //
        // LevelDoneForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 40);
        ClientSize = new Size(420, 190);
        ControlBox = false;
        Controls.Add(btnNext);
        Controls.Add(btnMenu);
        Controls.Add(btnRestart);
        Controls.Add(lblTitle);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "LevelDoneForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Level done";
        ResumeLayout(false);
    }

    #endregion

    private Label lblTitle;
    private Button btnRestart;
    private Button btnMenu;
    private Button btnNext;
}
