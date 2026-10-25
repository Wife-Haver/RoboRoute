namespace RoboRoute.Winforms.Screens;

partial class LevelSelect
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
        flowLevels = new FlowLayoutPanel();
        btnBack = new Button();
        SuspendLayout();
        //
        // lblTitle
        //
        lblTitle.Dock = DockStyle.Top;
        lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(1100, 80);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Select a level";
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        //
        // flowLevels
        //
        flowLevels.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        flowLevels.AutoScroll = true;
        flowLevels.Location = new Point(60, 100);
        flowLevels.Name = "flowLevels";
        flowLevels.Size = new Size(980, 420);
        flowLevels.TabIndex = 1;
        //
        // btnBack
        //
        btnBack.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnBack.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        btnBack.Location = new Point(60, 535);
        btnBack.Name = "btnBack";
        btnBack.Size = new Size(140, 45);
        btnBack.TabIndex = 2;
        btnBack.Text = "Back";
        btnBack.UseVisualStyleBackColor = true;
        btnBack.Click += btnBack_Click;
        //
        // LevelSelect
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 40);
        Controls.Add(btnBack);
        Controls.Add(flowLevels);
        Controls.Add(lblTitle);
        Name = "LevelSelect";
        Size = new Size(1100, 600);
        ResumeLayout(false);
    }

    #endregion

    private Label lblTitle;
    private FlowLayoutPanel flowLevels;
    private Button btnBack;
}
