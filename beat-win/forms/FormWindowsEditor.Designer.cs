namespace beat_win.forms;

partial class FormWindowsEditor
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWindowsEditor));
        menuStrip1 = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        toolStrip1 = new ToolStrip();
        save = new ToolStripButton();
        open = new ToolStripButton();
        help = new ToolStripButton();
        editor = new EditorDrawingControl();
        statusStrip1 = new StatusStrip();
        menuStrip1.SuspendLayout();
        toolStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip1
        // 
        menuStrip1.BackColor = SystemColors.MenuBar;
        menuStrip1.ImageScalingSize = new Size(24, 24);
        menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(800, 36);
        menuStrip1.TabIndex = 0;
        menuStrip1.Text = "menuStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(54, 30);
        fileToolStripMenuItem.Text = "File";
        // 
        // toolStrip1
        // 
        toolStrip1.BackColor = SystemColors.MenuBar;
        toolStrip1.Font = new Font("Segoe Fluent Icons", 9F);
        toolStrip1.ImageScalingSize = new Size(24, 24);
        toolStrip1.Items.AddRange(new ToolStripItem[] { save, open, help });
        toolStrip1.Location = new Point(0, 36);
        toolStrip1.Name = "toolStrip1";
        toolStrip1.Size = new Size(800, 38);
        toolStrip1.TabIndex = 1;
        toolStrip1.Text = "toolStrip1";
        // 
        // save
        // 
        save.DisplayStyle = ToolStripItemDisplayStyle.Text;
        save.Font = new Font("Segoe Fluent Icons", 9F);
        save.Image = (Image)resources.GetObject("save.Image");
        save.ImageTransparentColor = Color.Magenta;
        save.Name = "save";
        save.Size = new Size(34, 33);
        save.Text = "";
        save.Click += save_Click;
        // 
        // open
        // 
        open.DisplayStyle = ToolStripItemDisplayStyle.Text;
        open.Image = (Image)resources.GetObject("open.Image");
        open.ImageTransparentColor = Color.Magenta;
        open.Name = "open";
        open.Size = new Size(34, 33);
        open.Text = "";
        open.Click += open_Click;
        // 
        // help
        // 
        help.DisplayStyle = ToolStripItemDisplayStyle.Text;
        help.Image = (Image)resources.GetObject("help.Image");
        help.ImageTransparentColor = Color.Magenta;
        help.Name = "help";
        help.Size = new Size(34, 33);
        help.Text = "";
        help.Click += help_Click;
        // 
        // editor
        // 
        editor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        editor.Cursor = Cursors.IBeam;
        editor.Location = new Point(0, 77);
        editor.Name = "editor";
        editor.Size = new Size(800, 348);
        editor.TabIndex = 2;
        editor.Text = "editorDrawingControl1";
        editor.Click += editor_Click;
        editor.Paint += editor_Paint;
        editor.Enter += editor_Enter;
        editor.KeyPress += editor_KeyPress;
        editor.Leave += editor_Leave;
        editor.PreviewKeyDown += editor_KeyDown;
        // 
        // statusStrip1
        // 
        statusStrip1.ImageScalingSize = new Size(24, 24);
        statusStrip1.Location = new Point(0, 428);
        statusStrip1.Name = "statusStrip1";
        statusStrip1.Size = new Size(800, 22);
        statusStrip1.TabIndex = 3;
        statusStrip1.Text = "statusStrip1";
        // 
        // FormWindowsEditor
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.Window;
        ClientSize = new Size(800, 450);
        Controls.Add(statusStrip1);
        Controls.Add(editor);
        Controls.Add(toolStrip1);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        Name = "FormWindowsEditor";
        Text = "(beat)";
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        toolStrip1.ResumeLayout(false);
        toolStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip1;
    private ToolStrip toolStrip1;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripButton save;
    private ToolStripButton open;
    private ToolStripButton help;
    private EditorDrawingControl editor;
    private StatusStrip statusStrip1;
}