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
        lblResult = new Label();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(23, 27);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(434, 80);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Level done!";
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // btnRestart
        // 
        btnRestart.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnRestart.Location = new Point(23, 147);
        btnRestart.Margin = new Padding(3, 4, 3, 4);
        btnRestart.Name = "btnRestart";
        btnRestart.Size = new Size(131, 67);
        btnRestart.TabIndex = 1;
        btnRestart.Text = "Restart";
        btnRestart.UseVisualStyleBackColor = true;
        btnRestart.Click += btnRestart_Click;
        // 
        // btnMenu
        // 
        btnMenu.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnMenu.Location = new Point(174, 147);
        btnMenu.Margin = new Padding(3, 4, 3, 4);
        btnMenu.Name = "btnMenu";
        btnMenu.Size = new Size(131, 67);
        btnMenu.TabIndex = 2;
        btnMenu.Text = "Main menu";
        btnMenu.UseVisualStyleBackColor = true;
        btnMenu.Click += btnMenu_Click;
        // 
        // btnNext
        // 
        btnNext.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnNext.Location = new Point(326, 147);
        btnNext.Margin = new Padding(3, 4, 3, 4);
        btnNext.Name = "btnNext";
        btnNext.Size = new Size(131, 67);
        btnNext.TabIndex = 3;
        btnNext.Text = "Next level";
        btnNext.UseVisualStyleBackColor = true;
        btnNext.Click += btnNext_Click;
        // 
        // lblResult
        // 
        lblResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
        lblResult.AutoSize = true;
        lblResult.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblResult.ForeColor = SystemColors.Control;
        lblResult.Location = new Point(204, 107);
        lblResult.Name = "lblResult";
        lblResult.Size = new Size(51, 23);
        lblResult.TabIndex = 4;
        lblResult.Text = "score";
        lblResult.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // LevelDoneForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 40);
        ClientSize = new Size(480, 253);
        ControlBox = false;
        Controls.Add(lblResult);
        Controls.Add(btnNext);
        Controls.Add(btnMenu);
        Controls.Add(btnRestart);
        Controls.Add(lblTitle);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Margin = new Padding(3, 4, 3, 4);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "LevelDoneForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Level done";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblTitle;
    private Button btnRestart;
    private Button btnMenu;
    private Button btnNext;
    private Label lblResult;
}
