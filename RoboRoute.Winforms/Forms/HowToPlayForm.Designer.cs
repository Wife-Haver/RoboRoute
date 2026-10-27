namespace RoboRoute.Winforms.Screens;

partial class HowToPlayForm
{
    private System.ComponentModel.IContainer? components = null;

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        picHelp = new PictureBox();
        txtHelp = new TextBox();
        btnClose = new Button();
        ((System.ComponentModel.ISupportInitialize)picHelp).BeginInit();
        SuspendLayout();
        // 
        // picHelp
        // 
        picHelp.BackColor = Color.FromArgb(24, 24, 32);
        picHelp.Location = new Point(23, 27);
        picHelp.Margin = new Padding(3, 4, 3, 4);
        picHelp.Name = "picHelp";
        picHelp.Size = new Size(594, 293);
        picHelp.SizeMode = PictureBoxSizeMode.Zoom;
        picHelp.TabIndex = 0;
        picHelp.TabStop = false;
        // 
        // txtHelp
        // 
        txtHelp.BackColor = Color.FromArgb(24, 24, 32);
        txtHelp.BorderStyle = BorderStyle.None;
        txtHelp.Font = new Font("Segoe UI", 10.5F);
        txtHelp.ForeColor = Color.White;
        txtHelp.Location = new Point(23, 340);
        txtHelp.Margin = new Padding(3, 4, 3, 4);
        txtHelp.Multiline = true;
        txtHelp.Name = "txtHelp";
        txtHelp.ReadOnly = true;
        txtHelp.ScrollBars = ScrollBars.Vertical;
        txtHelp.Size = new Size(594, 273);
        txtHelp.TabIndex = 1;
        txtHelp.TabStop = false;
        // 
        // btnClose
        // 
        btnClose.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnClose.Location = new Point(251, 633);
        btnClose.Margin = new Padding(3, 4, 3, 4);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(137, 53);
        btnClose.TabIndex = 2;
        btnClose.Text = "Close";
        btnClose.UseVisualStyleBackColor = true;
        btnClose.Click += btnClose_Click;
        // 
        // HowToPlayForm
        // 
        AcceptButton = btnClose;
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 40);
        CancelButton = btnClose;
        ClientSize = new Size(640, 713);
        Controls.Add(btnClose);
        Controls.Add(txtHelp);
        Controls.Add(picHelp);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        Margin = new Padding(3, 4, 3, 4);
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "HowToPlayForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "How to play";
        ((System.ComponentModel.ISupportInitialize)picHelp).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private PictureBox picHelp;
    private TextBox txtHelp;
    private Button btnClose;
}
