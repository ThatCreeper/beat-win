using Raylib_cs;
using Windows.Win32;

namespace beat_win;

internal static class Program
{
    static Document document = new();
    static bool needsRedraw = true;

    static int cursorRow = 0;
    static int cursorIdx = 0;
    static int cursorUpDownX = -1;
    static bool mouseVisible = true;
    static float scroll = -GUI.TopPad;
    static float maxScroll = 10_000;
    static int scrollMinVisible = 0;

    static float renderCursorX = 0;
    static float renderCursorY = 0;

    static Line CurrentLine => document.Lines[cursorRow];

    static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(GUI.Inch(10), GUI.Inch(6), "(beat)");
        Raylib.EnableEventWaiting();
        SetTitleDark(true);

        GUI.Load();
        ResizeCenter(GUI.Inch(10), GUI.Inch(6));

        while (!Raylib.WindowShouldClose())
        {
            UpdateScroll();
            UpdateMouse();

            LineGeneralInput();
            if (LineRowInput())
            {
                string removed = document.Remove(cursorRow);
                cursorRow--;
                cursorIdx = CurrentLine.MaxIdx;
                document.Alter(cursorRow, mut => mut.AddStringEnd(removed));
                needsRedraw = true;
            }

            ClampScroll();

            if (needsRedraw)
            {
                UpdateMaxScroll();
                Render();
            }
            else
            {
                Raylib.PollInputEvents();
            }
        }

        Raylib.CloseWindow();
    }

    static void LineGeneralInput()
    {
        Line line = document.Lines[cursorRow];
        int maxIdx = line.MaxIdx;
        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            string next = document.Alter(cursorRow, mut => mut.RemoveAllAfterPosition(cursorIdx));
            document.AddLine(cursorRow + 1, next);

            cursorRow++;
            cursorIdx = 0;
            cursorUpDownX = -1;
            needsRedraw = true;
            return;
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Tab))
        {
            if (line.Kind == LineKind.Character || line.Kind == LineKind.Dialogue)
            {
                document.AddLine(cursorRow + 1, "()");
                cursorRow++;
                cursorIdx = 1;
                cursorUpDownX = -1;
                needsRedraw = true;
            }
            else if (line.Kind == LineKind.Parenthetical)
            {
                document.AddLine(cursorRow + 1, "");
                cursorRow++;
                cursorIdx = 0;
                cursorUpDownX = -1;
                needsRedraw = true;
            }
        }

        if (KeyOrRepeat(KeyboardKey.Left))
        {
            cursorIdx--;
            if (cursorIdx < 0)
            {
                if (cursorRow > 0)
                {
                    cursorRow--;
                    cursorIdx = document.Lines[cursorRow].MaxIdx;
                }
                else
                {
                    cursorIdx = 0;
                }
            }
            cursorUpDownX = -1;
            needsRedraw = true;
            return;
        }
        if (KeyOrRepeat(KeyboardKey.Right))
        {
            cursorIdx++;
            if (cursorIdx > maxIdx)
            {
                if (document.Lines.Count > cursorRow + 1)
                {
                    cursorIdx = 0;
                    cursorRow++;
                }
                else
                {
                    cursorIdx = maxIdx;
                }
            }
            cursorUpDownX = -1;
            needsRedraw = true;
            return;
        }
        if (KeyOrRepeat(KeyboardKey.Up))
        {
            int lineY = CurrentLine.GetCursorSublineY(cursorIdx);
            if (cursorRow == 0 && lineY == 0)
            {
                cursorIdx = 0;
            }
            else
            {
                cursorUpDownX = cursorUpDownX != -1 ? cursorUpDownX : CurrentLine.GetCursorCharX(cursorIdx);
                if (lineY == 0)
                {
                    cursorRow--;
                    cursorIdx = CurrentLine.GetIndex(cursorUpDownX, CurrentLine.RowCount - 1);
                }
                else
                {
                    cursorIdx = CurrentLine.GetIndex(cursorUpDownX, lineY - 1);
                }
            }
            needsRedraw = true;
            return;
        }
        if (KeyOrRepeat(KeyboardKey.Down))
        {
            int lineY = CurrentLine.GetCursorSublineY(cursorIdx);
            if (cursorRow == document.Lines.Count - 1 && lineY == CurrentLine.RowCount - 1)
            {
                cursorIdx = CurrentLine.MaxIdx;
            }
            else
            {
                cursorUpDownX = cursorUpDownX != -1 ? cursorUpDownX : CurrentLine.GetCursorCharX(cursorIdx);
                if (lineY == CurrentLine.RowCount - 1)
                {
                    cursorRow++;
                    cursorIdx = CurrentLine.GetIndex(cursorUpDownX, 0);
                }
                else
                {
                    cursorIdx = CurrentLine.GetIndex(cursorUpDownX, lineY + 1);
                }
            }
            needsRedraw = true;
            return;
        }
    }

    // Returns true if the line should be removed
    static bool LineRowInput()
    {
        Line currentLine = document.Lines[cursorRow];
        int input;
        while ((input = Raylib.GetCharPressed()) != 0)
        {
            if (input < 32 || input > 125) continue;
            document.Alter(cursorRow, mut => mut.AddCharacter(cursorIdx, (char)input));
            cursorIdx++;
            cursorUpDownX = -1;
            needsRedraw = true;
            mouseVisible = false;
        }
        if (KeyOrRepeat(KeyboardKey.Backspace))
        {
            mouseVisible = false;
            cursorUpDownX = -1;
            if (cursorIdx == 0)
            {
                return cursorRow != 0;
            }
            else
            {
                document.Alter(cursorRow, mut => mut.RemoveCharacterBackwards(cursorIdx));
                cursorIdx--;
                needsRedraw = true;
            }
        }
        return false;
    }

    static int ScreenHeight => Raylib.GetScreenHeight();
    static int ScreenWidth => Raylib.GetRenderWidth();
    static int PageWidth => GUI.Inch(8.5f);
    static int PageLeftPad => (ScreenWidth - PageWidth) / 2;

    static unsafe void SetTitleDark(bool dark)
    {
        int value = dark ? 1 : 0;
        PInvoke.DwmSetWindowAttribute(
            (Windows.Win32.Foundation.HWND)Raylib.GetWindowHandle(),
            Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
            &value,
            sizeof(int));
    }

    static void ResizeCenter(int width, int height)
    {
        var oldWidth = Raylib.GetScreenWidth();
        var oldHeight = Raylib.GetScreenHeight();
        var oldPos = Raylib.GetWindowPosition();
        Raylib.SetWindowSize(width, height);
        Raylib.SetWindowPosition(
            (int)oldPos.X + (oldWidth - width) / 2,
            (int)oldPos.Y + (oldHeight - height) / 2);
    }

    static bool KeyOrRepeat(KeyboardKey key)
    {
        return Raylib.IsKeyPressed(key) || Raylib.IsKeyPressedRepeat(key);
    }

    static void UpdateScroll()
    {
        var scr = -GUI.FloatInch(Raylib.GetMouseWheelMoveV().Y * 0.5f);
        if (scr == 0) return;
        
        scroll += scr;
        needsRedraw = true;

        while (scroll < 0)
        {
            if (scrollMinVisible == 0)
            {
                scroll = Math.Max(-GUI.TopPad, scroll);
                break;
            }
            else
            {
                scrollMinVisible--;
                scroll += GUI.TextSize * document.Lines[scrollMinVisible].RowCount;
            }
        }
        Line minVisibleLine = document.Lines[scrollMinVisible];
        float scrollMinHeight = GUI.TextSize * (minVisibleLine.GlobalRow + minVisibleLine.RowCount);
        if (scroll > maxScroll - scrollMinHeight)
        {
            scroll = Math.Min(scroll, maxScroll - scrollMinHeight);
        }
        while (scroll > GUI.TextSize * document.Lines[scrollMinVisible].RowCount)
        {
            if (scrollMinVisible >= document.Lines.Count - 1)
            {
                break;
            }
            scroll -= GUI.TextSize * (document.Lines[scrollMinVisible].RowCount);
            scrollMinVisible++;
        }
    }

    static float GetScrollPX()
    {
        Line minVisibleLine = document.Lines[scrollMinVisible];
        return scroll + GUI.TextSize * (minVisibleLine.GlobalRow + minVisibleLine.RowCount);
    }

    static int GetMaxVisibleLines()
    {
        return Math.Min(
            (int)Math.Ceiling((float)ScreenHeight / GUI.TextSize) + 2,
            document.Lines.Count - scrollMinVisible);
    }

    // Should make me not ever have to deal with this.
    static void ClampScroll()
    {
        scrollMinVisible = Math.Clamp(scrollMinVisible, 0, document.Lines.Count - 1);
    }

    static void UpdateMouse()
    {
        var pageWidth = PageWidth;
        var pageLeftPad = PageLeftPad;
        var mX = Raylib.GetMouseX() - pageLeftPad;
        var mY = Raylib.GetMouseY() + (int)GetScrollPX();

        if (Raylib.GetMouseDelta().LengthSquared() > 0)
        {
            mouseVisible = true;
        }

        if (mouseVisible)
        {
            Raylib.ShowCursor();
            if (mX >= 0 && mX < pageWidth && mY >= 0)
            {
                Raylib.SetMouseCursor(MouseCursor.IBeam);

                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    SelectPixelXY(mX, mY);
                }
            }
            else
            {
                Raylib.SetMouseCursor(MouseCursor.Default);
            }
        }
        else
        {
            Raylib.HideCursor();
        }
    }

    static void SelectPixelXY(int x, int y)
    {
        int pY = y / GUI.TextSize;
        int row = 0;
        bool hit = false;
        for (int i = 0; i < document.Lines.Count; i++)
        {
            Line line = document.Lines[i];
            int rc = line.RowCount;
            if (row + rc > pY)
            {
                cursorRow = i;
                cursorIdx = line.GetIndex(
                    Math.Max(0,(int)Math.Round((x - line.LeftPadPX) / GUI.CharacterWidth)),
                    pY - row);
                hit = true;
                break;
            }
            row += rc;
        }
        if (!hit)
        {
            cursorRow = document.Lines.Count - 1;
            cursorIdx = document.Lines[cursorRow].MaxIdx;
        }
        needsRedraw = true;
    }

    static void UpdateMaxScroll()
    {
        maxScroll = (document.TotalRows - 1) * GUI.TextSize;
    }


    // RENDER!

    static void Render()
    {
        var screenHeight = ScreenHeight;
        var pageWidth = PageWidth;
        var pageLeftPad = PageLeftPad;

        Raylib.BeginDrawing();
        Raylib.ClearBackground(GUI.UIBackground);
        Raylib.DrawRectangle(pageLeftPad, 0, pageWidth, screenHeight, GUI.Background);
        int drawnLines = document.Lines[scrollMinVisible].GlobalRow;
        for (int lineIdx = scrollMinVisible; lineIdx < scrollMinVisible + GetMaxVisibleLines(); lineIdx++)
        {
            Line line = document.Lines[lineIdx];
            // Page numbers!
            if (!line.PDFUnrenderedCache && (line.GlobalPDFRow % Document.LinesPerPage) == 0)
            {
                string sidebar = $"{(line.GlobalPDFRow / Document.LinesPerPage) + 1}";
                GUI.Text(
                    sidebar,
                    pageLeftPad + GUI.Inch(GUI.ActionLeftPad) + GUI.TextWidth(61),
                    GUI.TextSize * drawnLines - (int)GetScrollPX(),
                    false, true, false, GUI.Syntax);
            }
            // Scene numbers!
            if (line.Kind == LineKind.Scene)
            {
                string sidebar = $"{line.GlobalScene}";
                GUI.Text(
                    sidebar,
                    pageLeftPad + GUI.Inch(GUI.ActionLeftPad) - GUI.TextWidth(1 + sidebar.Length),
                    GUI.TextSize * drawnLines - (int)GetScrollPX(),
                    false, true, false, GUI.Syntax);
            }

            foreach (List<LineFragment> fragments in line.Content)
            {
                int xOffset = 0;
                foreach (LineFragment fragment in fragments)
                {
                    xOffset += GUI.Text(
                        fragment.Content,
                        pageLeftPad + line.LeftPadPX + xOffset,
                        GUI.TextSize * drawnLines - (int)GetScrollPX(),
                        fragment.Italic, fragment.Bold, fragment.Underline,
                        fragment.Syntax ? GUI.Syntax : line.Color);
                }
                drawnLines++;
            }
        }
        RenderCaret(pageLeftPad);
        RenderScrollBar();
        Raylib.EndDrawing();
        needsRedraw = false;
    }

    private static void RenderCaret(int pageLeftPad)
    {
        Line line = CurrentLine;
        int rows = line.GetCursorSublineY(cursorIdx);
        for (int i = 0; i < cursorRow; i++)
        {
            rows += document.Lines[i].RowCount;
        }

        int linePad = line.LeftPadPX;
        if (line.IsBlank && cursorRow != 0 && document.Lines[cursorRow - 1].DoesDialogueComeNext)
        {
            linePad = GUI.Inch(GUI.DialogueLeftPad);
        }

        Raylib.DrawRectangleRounded(
            new Rectangle(
                GUI.TextWidth(line.GetCursorCharX(cursorIdx)) + pageLeftPad + linePad - GUI.Point(1),
                rows* GUI.TextSize - GUI.Point(1.5f) - (int)GetScrollPX(),
                GUI.Point(1.5f),
                GUI.TextSize + GUI.Point(1)),
            0.8f,
            0,
            GUI.Cursor);
    }

    static int ScrollBarPadding => GUI.Point(5);

    static void RenderScrollBar()
    {
        float maxScrollDistance = maxScroll + GUI.TopPad;

        int scrollBarPadding = ScrollBarPadding;
        int scrollBarArea = ScreenHeight - scrollBarPadding * 2;
        int scrollBarWidth = GUI.Point(3);
        int scrollBarHeight = (int)(50 * scrollBarArea / maxScrollDistance);
        int scrollBarTopY = scrollBarPadding;
        int scrollBarBottomY = ScreenHeight - scrollBarPadding - scrollBarHeight;
        int scrollBarX = ScreenWidth - scrollBarPadding - scrollBarWidth;

        float scrollPercentage = (GetScrollPX() + GUI.TopPad) / maxScrollDistance;

        int scrollBarY = (int)MathHelpers.Lerp(scrollBarTopY, scrollBarBottomY, scrollPercentage);

        Raylib.DrawRectangleRounded(
            new Rectangle(
                scrollBarX,
                scrollBarY,
                scrollBarWidth,
                scrollBarHeight),
            0.8f,
            0,
            GUI.Background);
    }
}
