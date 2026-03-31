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

            if (needsRedraw)
            {
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
            if (cursorRow == 0)
            {
                cursorIdx = 0;
            }
            else
            {
                cursorUpDownX = cursorUpDownX != -1 ? cursorUpDownX : line.GetCursorCharX(cursorIdx);
                cursorRow--;
                line = document.Lines[cursorRow];
                cursorIdx = line.GetIndex(cursorUpDownX, line.RowCount - 1);
            }
            needsRedraw = true;
            return;
        }
        if (KeyOrRepeat(KeyboardKey.Down))
        {
            if (cursorRow == document.Lines.Count - 1)
            {
                cursorIdx = line.MaxIdx;
            }
            else
            {
                cursorUpDownX = cursorUpDownX != -1 ? cursorUpDownX : line.GetCursorCharX(cursorIdx);
                cursorRow++;
                line = document.Lines[cursorRow];
                cursorIdx = line.GetIndex(cursorUpDownX, line.RowCount - 1);
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

    static void Render()
    {
        var screenHeight = ScreenHeight;
        var pageWidth = PageWidth;
        var pageLeftPad = PageLeftPad;

        Raylib.BeginDrawing();
        Raylib.ClearBackground(GUI.UIBackground);
        Raylib.DrawRectangle(pageLeftPad, 0, pageWidth, screenHeight, GUI.Background);
        int drawnLines = 0;
        foreach (Line line in document.Lines)
        {
            string sidebar = $"{line.GlobalPDFRow}:{line.GlobalRow}";
            GUI.Text(sidebar, pageLeftPad + GUI.Inch(GUI.ActionLeftPad) - GUI.TextWidth(1 + sidebar.Length), GUI.TopPad + GUI.TextSize * drawnLines, false, false, false, true);
            foreach (string content in line.Content)
            {
                GUI.Text(content, pageLeftPad + line.LeftPadPX, GUI.TopPad + GUI.TextSize * drawnLines, false, false, false);
                drawnLines++;
            }
        }
        RenderCaret(pageLeftPad);
        Raylib.EndDrawing();
        needsRedraw = false;
    }

    private static void RenderCaret(int pageLeftPad)
    {
        Line line = document.Lines[cursorRow];
        int rows = line.GetCursorSublineY(cursorIdx);
        for (int i = 0; i < cursorRow; i++)
        {
            rows += document.Lines[i].RowCount;
        }
        Raylib.DrawRectangle(
            GUI.TextWidth(line.GetCursorCharX(cursorIdx)) + pageLeftPad + line.LeftPadPX,
            rows * GUI.TextSize + GUI.TopPad - GUI.Point(1.5f),
            GUI.Point(1.5f), GUI.TextSize + GUI.Point(1),
            GUI.Cursor);
    }

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

    static void UpdateMouse()
    {
        var pageWidth = PageWidth;
        var pageLeftPad = PageLeftPad;
        var mX = Raylib.GetMouseX() - pageLeftPad;
        var mY = Raylib.GetMouseY() - GUI.TopPad;

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
                cursorIdx = line.GetIndex((int)Math.Round((x - line.LeftPadPX) / GUI.CharacterWidth), pY - row);
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
}
