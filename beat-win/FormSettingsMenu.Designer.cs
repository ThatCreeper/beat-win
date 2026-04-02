namespace beat_win;

partial class FormSettingsMenu
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettingsMenu));
        tabControl1 = new TabControl();
        tabPage1 = new TabPage();
        richTextBox1 = new RichTextBox();
        label1 = new Label();
        tabPage2 = new TabPage();
        tabControl1.SuspendLayout();
        tabPage1.SuspendLayout();
        SuspendLayout();
        // 
        // tabControl1
        // 
        tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabControl1.Controls.Add(tabPage1);
        tabControl1.Controls.Add(tabPage2);
        tabControl1.Location = new Point(12, 12);
        tabControl1.Name = "tabControl1";
        tabControl1.SelectedIndex = 0;
        tabControl1.Size = new Size(776, 426);
        tabControl1.TabIndex = 0;
        // 
        // tabPage1
        // 
        tabPage1.Controls.Add(richTextBox1);
        tabPage1.Controls.Add(label1);
        tabPage1.Location = new Point(4, 34);
        tabPage1.Name = "tabPage1";
        tabPage1.Padding = new Padding(3);
        tabPage1.Size = new Size(768, 388);
        tabPage1.TabIndex = 0;
        tabPage1.Text = "Credits";
        tabPage1.UseVisualStyleBackColor = true;
        // 
        // richTextBox1
        // 
        richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        richTextBox1.Location = new Point(6, 31);
        richTextBox1.Name = "richTextBox1";
        richTextBox1.ReadOnly = true;
        richTextBox1.Size = new Size(756, 351);
        richTextBox1.TabIndex = 2;
        richTextBox1.Text = resources.GetString("richTextBox1.Text");
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(6, 3);
        label1.Name = "label1";
        label1.Size = new Size(57, 25);
        label1.TabIndex = 0;
        label1.Text = "(beat)";
        // 
        // tabPage2
        // 
        tabPage2.Location = new Point(4, 34);
        tabPage2.Name = "tabPage2";
        tabPage2.Padding = new Padding(3);
        tabPage2.Size = new Size(768, 388);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "Main Configuration";
        tabPage2.UseVisualStyleBackColor = true;
        // 
        // FormSettingsMenu
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(tabControl1);
        Name = "FormSettingsMenu";
        Text = "(beat) Settings";
        tabControl1.ResumeLayout(false);
        tabPage1.ResumeLayout(false);
        tabPage1.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TabControl tabControl1;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private RichTextBox richTextBox1;
    private Label label1;
}