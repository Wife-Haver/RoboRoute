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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LevelSelect));
        lblTitle = new Label();
        flowLevels = new FlowLayoutPanel();
        btnBack = new Button();
        HowToPlayBtn = new Button();
        ShowSaveBtn = new Button();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.BackColor = Color.Transparent;
        lblTitle.Dock = DockStyle.Top;
        lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(0, 0);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(1257, 107);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Select a level";
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // flowLevels
        // 
        flowLevels.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        flowLevels.AutoScroll = true;
        flowLevels.BackColor = Color.Transparent;
        flowLevels.Location = new Point(69, 133);
        flowLevels.Margin = new Padding(3, 4, 3, 4);
        flowLevels.Name = "flowLevels";
        flowLevels.Size = new Size(1120, 560);
        flowLevels.TabIndex = 1;
        // 
        // btnBack
        // 
        btnBack.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnBack.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        btnBack.Location = new Point(69, 713);
        btnBack.Margin = new Padding(3, 4, 3, 4);
        btnBack.Name = "btnBack";
        btnBack.Size = new Size(160, 60);
        btnBack.TabIndex = 2;
        btnBack.Text = "Back";
        btnBack.UseVisualStyleBackColor = true;
        btnBack.Click += btnBack_Click;
        // 
        // HowToPlayBtn
        // 
        HowToPlayBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        HowToPlayBtn.Image = (Image)resources.GetObject("HowToPlayBtn.Image");
        HowToPlayBtn.Location = new Point(1121, 713);
        HowToPlayBtn.Name = "HowToPlayBtn";
        HowToPlayBtn.Size = new Size(56, 60);
        HowToPlayBtn.TabIndex = 3;
        HowToPlayBtn.UseVisualStyleBackColor = true;
        HowToPlayBtn.Click += HowToPlayBtn_Click;
        // 
        // ShowSaveBtn
        // 
        ShowSaveBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        ShowSaveBtn.Location = new Point(1160, 12);
        ShowSaveBtn.Name = "ShowSaveBtn";
        ShowSaveBtn.Size = new Size(94, 29);
        ShowSaveBtn.TabIndex = 4;
        ShowSaveBtn.Text = "Show Save";
        ShowSaveBtn.UseVisualStyleBackColor = true;
        ShowSaveBtn.Click += ShowSaveBtn_Click;
        // 
        // LevelSelect
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 40);
        BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
        Controls.Add(ShowSaveBtn);
        Controls.Add(HowToPlayBtn);
        Controls.Add(btnBack);
        Controls.Add(flowLevels);
        Controls.Add(lblTitle);
        Margin = new Padding(3, 4, 3, 4);
        Name = "LevelSelect";
        Size = new Size(1257, 800);
        ResumeLayout(false);
    }

    #endregion

    private Label lblTitle;
    private FlowLayoutPanel flowLevels;
    private Button btnBack;
    private Button HowToPlayBtn;
    private Button ShowSaveBtn;
}
