using beat_win.doc;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;

namespace beat_win;

public abstract class ScrollAwareTextEditorRenderer : TextEditorRenderer
{
    public int ScrollMinVisible = 0;
    public int ScrollPixels = -GUI.TopPad;
    public float PartialScroll = -GUI.TopPad;
    public int MaxScroll = 0;

    public void ResetScroll()
    {
        ScrollMinVisible = 0;
        PartialScroll = -GUI.TopPad;
    }

    protected void UpdateMaxScroll(Document document)
    {
        MaxScroll = (document.TotalRows - 1) * GUI.TextSize;
    }

    private float GetScrollPX(Document document)
    {
        Line minVisibleLine = document.Lines[ScrollMinVisible];
        return PartialScroll + GUI.TextSize * (minVisibleLine.GlobalRow);
    }

    public void UpdateScroll(Document document, float motion)
    {
        if (motion == 0) return;

        PartialScroll += motion;
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
        ScrollPixels = (int)GetScrollPX(document);
    }
}
