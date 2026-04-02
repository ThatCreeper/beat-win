using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace beat_win.forms;

public partial class FormWindowsEditor : Form
{
    public WindowsTextEditorRenderer Renderer;

    public FormWindowsEditor(WindowsTextEditorRenderer renderer)
    {
        InitializeComponent();

        Renderer = renderer;
        editor.IsReal = true;
    }

    private void save_Click(object sender, EventArgs e)
    {
        Renderer.CommandCharacter = 'S';
        Renderer.OnInputCommandCharacter?.Invoke();
    }

    private void help_Click(object sender, EventArgs e)
    {
        Renderer.CommandCharacter = 'M';
        Renderer.OnInputCommandCharacter?.Invoke();
    }

    private void open_Click(object sender, EventArgs e)
    {
        Renderer.CommandCharacter = 'O';
        Renderer.OnInputCommandCharacter?.Invoke();
    }
}
