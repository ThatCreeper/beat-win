using System;
using System.Collections.Generic;
using System.Text;

namespace beat_win.forms;

public class EditorDrawingControl : ScrollableControl
{
    public bool IsReal = false;

    protected override void OnCreateControl()
    {
        AutoScroll = true;
        VScroll = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();
        base.OnCreateControl();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!IsReal)
        {
            Rectangle rect = ClientRectangle;
            //rect.Width--;
            //rect.Height--;

            using Pen pen = new Pen(Color.White);
            using Brush red = new SolidBrush(Color.Red);

            e.Graphics.FillRectangle(red, rect);
            e.Graphics.DrawLine(pen, new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Bottom));
            e.Graphics.DrawLine(pen, new Point(rect.Right, rect.Top), new Point(rect.Left, rect.Bottom));
        }
        base.OnPaint(e);
    }

    protected override void OnClientSizeChanged(EventArgs e)
    {
        Invalidate();
        base.OnClientSizeChanged(e);
    }
}
