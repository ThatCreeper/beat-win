using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace beat_win.forms;

public partial class FormWindowsEditor : Form
{
    public WindowsTextEditorRenderer Renderer;

    public FormWindowsEditor(WindowsTextEditorRenderer renderer)
    {
        InitializeComponent();

        Renderer = renderer;
        editor.IsReal = true;

        editor.GotFocus += editor_GotFocus;
        editor.LostFocus += editor_LostFocus;
        editor.Scroll += editor_Scroll;
        editor.MouseWheel += editor_MouseWheel;
        editor.MouseMove += editor_MouseMove;
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

    private void editor_Paint(object sender, PaintEventArgs e)
    {
        Renderer.PaintEvent = e;
        Renderer.DrawRect = editor.ClientRectangle;
        Renderer.OnRequestRender?.Invoke();
        editor.AutoScrollMinSize = new Size(0, Renderer.MaxScroll + editor.ClientRectangle.Height + GUI.TopPad);
        editor.VerticalScroll.Value = Renderer.ScrollPixels + GUI.TopPad;
    }

    public void InvalidateEditor()
    {
        editor.Invalidate();
    }

    private void editor_KeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        if (e.Control)
        {
            Renderer.CommandCharacter = (char)e.KeyValue;
            Renderer.OnInputCommandCharacter?.Invoke();
        }
        else
        {
            if (e.KeyCode == Keys.Left)
                Renderer.OnLeft?.Invoke(e.Shift);
            else if (e.KeyCode == Keys.Right)
                Renderer.OnRight?.Invoke(e.Shift);
            else if (e.KeyCode == Keys.Up)
                Renderer.OnUp?.Invoke(e.Shift);
            else if (e.KeyCode == Keys.Down)
                Renderer.OnDown?.Invoke(e.Shift);
            else if (e.KeyCode == Keys.Tab)
                Renderer.OnTab?.Invoke();
            else if (e.KeyCode == Keys.Enter)
                Renderer.OnEnter?.Invoke();
            else if (e.KeyCode == Keys.Back)
                Renderer.OnBackspace?.Invoke();
        }
    }

    private void editor_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (Char.IsControl(e.KeyChar)) return;
        Renderer.OnInputCharacter?.Invoke(e.KeyChar.ToString());
    }

    private void editor_Click(object sender, EventArgs e)
    {
        editor.Focus();
        var point = editor.PointToClient(Cursor.Position);
        Renderer.Click(
            point.X,
            point.Y,
            editor.ClientRectangle.Width);
    }

    private void editor_Enter(object sender, EventArgs e)
    {
    }

    private void editor_Leave(object sender, EventArgs e)
    {
    }

    private void editor_GotFocus(object? sender, EventArgs e)
    {
        PInvoke.CreateCaret((HWND)editor.Handle, null, 1, GUI.TextSize);
        //PInvoke.SetCaretPos(30, 30);
        PInvoke.ShowCaret();
        InvalidateEditor();
    }

    private void editor_LostFocus(object? sender, EventArgs e)
    {
        PInvoke.DestroyCaret();
    }

    private void editor_Scroll(object? sender, ScrollEventArgs e)
    {
        Renderer.OnScrollEvent?.Invoke(e.NewValue - e.OldValue);
    }

    private void editor_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (e.Delta > 0 && Renderer.ScrollPixels == -GUI.TopPad) return;
        if (e.Delta < 0 && Renderer.ScrollPixels == Renderer.MaxScroll) return;
        if (e.Delta == 0) return;
        Renderer.OnScrollEvent?.Invoke(-e.Delta);
    }

    private void editor_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!editor.Focused) return;
        var point = editor.PointToClient(Cursor.Position);
        if (e.Button == MouseButtons.Left)
            Renderer.Drag(point.X, point.Y, editor.ClientRectangle.Width);
    }
}
