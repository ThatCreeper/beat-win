using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace beat_win;

public abstract class ScrollAwareTextEditorRenderer : ITextEditorRenderer
{
    public bool NeedsRender => needsRender;
    bool needsRender = true;

    public int ScrollMinVisible = 0;
    public float PartialScroll = -GUI.TopPad;
    public int MaxScroll = 0;

    public void MarkDirty()
    {
        needsRender = true;
    }
    public void MarkNotDirty()
    {
        needsRender = false;
    }
    public abstract int Text(string text, int x, int y, bool italic, bool bold, bool underline, Raylib_cs.Color color, bool syntax);

    public void ResetScroll()
    {
        ScrollMinVisible = 0;
        PartialScroll = -GUI.TopPad;
    }

    public void UpdateMaxScroll(Document document)
    {
        MaxScroll = (document.TotalRows - 1) * GUI.TextSize;
    }

    public float GetScrollPX(Document document)
    {
        Line minVisibleLine = document.Lines[ScrollMinVisible];
        return PartialScroll + GUI.TextSize * (minVisibleLine.GlobalRow);
    }

    public void UpdateScroll(Document document)
    {
        var scr = -GUI.FloatInch(Raylib.GetMouseWheelMoveV().Y * 0.5f);
        if (scr == 0) return;

        PartialScroll += scr;
        MarkDirty();

        while (PartialScroll < 0)
        {
            if (ScrollMinVisible == 0)
            {
                PartialScroll = Math.Max(-GUI.TopPad, PartialScroll);
                break;
            }
            else
            {
                ScrollMinVisible--;
                PartialScroll += GUI.TextSize * document.Lines[ScrollMinVisible].RowCount;
            }
        }
        Line minVisibleLine = document.Lines[ScrollMinVisible];
        float scrollMinHeight = GUI.TextSize * minVisibleLine.GlobalRow;
        if (PartialScroll > MaxScroll - scrollMinHeight)
        {
            PartialScroll = Math.Min(PartialScroll, MaxScroll - scrollMinHeight);
        }
        while (PartialScroll > GUI.TextSize * document.Lines[ScrollMinVisible].RowCount)
        {
            if (ScrollMinVisible >= document.Lines.Count - 1)
            {
                break;
            }
            PartialScroll -= GUI.TextSize * (document.Lines[ScrollMinVisible].RowCount);
            ScrollMinVisible++;
        }
    }
}
