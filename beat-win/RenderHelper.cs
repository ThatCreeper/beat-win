using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using Color = Raylib_cs.Color;
using Font = Raylib_cs.Font;

namespace beat_win;

public static class RenderHelper
{
    public static bool NeedsRender = true;

    public static int Text(string text, int x, int y, bool italic, bool bold, bool underline, Color color, bool syntax)
    {
        Font font = GUI.GetFont(italic, bold);
        int size = GUI.TextSize;
        float chWid = GUI.CharacterWidth;
        int width = GUI.TextWidth(text);
        if (underline)
        {
            Raylib.DrawRectangle(x - GUI.Point(2), y + size, width + GUI.Point(4), GUI.Point(0.5f), color);
        }
        for (int i = 0; i < text.Length; i++)
        {
            Raylib.DrawTextCodepoint(font, text[i], new System.Numerics.Vector2(x + chWid * i, y), size, syntax ? GUI.Syntax : color);
        }
        return width;
    }
}
