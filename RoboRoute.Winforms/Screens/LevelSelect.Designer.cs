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
        btnImport = new Button();
        openLevelDialog = new OpenFileDialog();
        SuspendLayout();
        // 
        // lblTitle
        // 
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
        // btnImport
        // 
        btnImport.Location = new Point(1029, 716);
        btnImport.Name = "btnImport";
        btnImport.Size = new Size(160, 60);
        btnImport.TabIndex = 3;
        btnImport.Text = "button1";
        btnImport.UseVisualStyleBackColor = true;
        btnImport.Click += btnImport_Click;
        // 
        // openLevelDialog
        // 
        openLevelDialog.Filter = "Level files (*.txt)|*.txt|All files (*.*)|*.*";
        openLevelDialog.Title = "Import a level";
        openLevelDialog.FileOk += openFileDialog1_FileOk;
        // 
        // LevelSelect
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(30, 30, 40);
        Controls.Add(btnImport);
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
    private Button btnImport;
    private OpenFileDialog openLevelDialog;
}
