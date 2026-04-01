using Raylib_cs;
using Windows.Win32;
using static QuestPDF.Helpers.Colors;
using Rectangle = Raylib_cs.Rectangle;

namespace beat_win;

internal static class Program
{
    static Document document = new();
    static Caret caret = new(0, 0, document);

    static bool mouseVisible = true;
    static float scroll = -GUI.TopPad;
    static float maxScroll = 10_000;
    static int scrollMinVisible = 0;


    [STAThread]
    static void Main(string[] args)
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(GUI.Inch(10), GUI.Inch(6), "(beat)");
        Raylib.EnableEventWaiting();
        SetTitleDark(true);

        GUI.Load();
        ResizeCenter(GUI.Inch(10), GUI.Inch(6));

        document.InsertMultilineAtCaret(0, 0, "No file is loaded.\n\nDrop a file onto the window to continue.\n\nChanges in this document will not and can not be saved.");

        while (!Raylib.WindowShouldClose())
        {
            FileLoader();

            UpdateScroll();
            UpdateMouse();

            LineGeneralInput();
            if (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl))
            {
                SpecialInput();
            }
            else
            {
                LineRowInput();
            }

            ClampScroll();

            if (RenderHelper.NeedsRender)
            {
                UpdateMaxScroll();
                Render();
                SetTitleBarText();
            }
            else
            {
                Raylib.PollInputEvents();
            }
        }

        document.AskSave();

        Raylib.CloseWindow();
    }

    static void SetTitleBarText()
    {
        Raylib.SetWindowTitle($"(beat) {document.Name()} {(document.Edited ? "(Unsaved)" : "")}");
    }

    static void LineGeneralInput()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            caret.Insert("\n");
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Tab))
        {
            Line line = caret.EndLine;
            if (line.Kind == LineKind.Character || line.Kind == LineKind.Dialogue)
            {
                document.AddLine(caret.SelectionEnd.Row + 1, "()");
                caret.SetCaretIndex(caret.SelectionEnd.Row, 1);
            }
            else if (line.Kind == LineKind.Parenthetical)
            {
                document.AddLine(caret.SelectionEnd.Row + 1, "");
                caret.SetCaretIndex(caret.SelectionEnd.Row, 0);
            }
        }

        bool shifting = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);

        if (KeyOrRepeat(KeyboardKey.Left))
        {
            if (shifting)
                caret.MoveEndX(-1);
            else
                caret.MoveCaretX(-1);
        }
        if (KeyOrRepeat(KeyboardKey.Right))
        {
            if (shifting)
                caret.MoveEndX(1);
            else
                caret.MoveCaretX(1);
        }
        if (KeyOrRepeat(KeyboardKey.Up))
        {
            if (shifting)
                caret.MoveEndY(-1);
            else
                caret.MoveCaretY(-1);
        }
        if (KeyOrRepeat(KeyboardKey.Down))
        {
            if (shifting)
                caret.MoveEndY(1);
            else
                caret.MoveCaretY(1);
        }
    }

    static void SpecialInput()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.S))
        {
            document.Save();
        }
        if (Raylib.IsKeyPressed(KeyboardKey.N))
        {
            FileDocument? doc = FileDocument.NewWithDialog();
            if (doc != null) LoadDocument(doc);
        }
        if (Raylib.IsKeyPressed(KeyboardKey.O))
        {
            FileDocument? doc = FileDocument.OpenWithDialog();
            if (doc != null) LoadDocument(doc);
        }
    }

    static void LoadDocument(Document doc)
    {
        scrollMinVisible = 0;
        scroll = -GUI.TopPad;
        RenderHelper.NeedsRender = true;
        document.AskSave();
        document = doc;
        caret = new Caret(0, 0, document);
    }

    static void FileLoader()
    {
        if (Raylib.IsFileDropped())
        {
            string[] files = Raylib.GetDroppedFiles();
            if (files.Length == 0) return;
            LoadDocument(new FileDocument(files[0]));
        }
    }

    // Returns true if the line should be removed
    static void LineRowInput()
    {
        int input;
        while ((input = Raylib.GetCharPressed()) != 0)
        {
            if (input < 32 || input > 125) continue;
            caret.Insert((char)input);
            mouseVisible = false;
        }
        if (KeyOrRepeat(KeyboardKey.Backspace))
        {
            mouseVisible = false;
            caret.DeleteBackspace();
        }
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
        RenderHelper.NeedsRender = true;

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
        float scrollMinHeight = GUI.TextSize * minVisibleLine.GlobalRow;
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
        return scroll + GUI.TextSize * (minVisibleLine.GlobalRow);
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
                    SelectPixelXY(mX, mY, Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift), scrollMinVisible);
                }
                else if (Raylib.IsMouseButtonDown(MouseButton.Left))
                {
                    SelectPixelXY(mX, mY, true, scrollMinVisible);
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

    // x and y should be in terms of page pixels
    static void SelectPixelXY(int x, int y, bool shifting, int startSearchingRow = 0)
    {
        int row = y / GUI.TextSize;

        row = Math.Clamp(row, 0, document.TotalRows - 1);

        for (int i = startSearchingRow; i < document.Lines.Count; i++)
        {
            Line line = document.Lines[i];
            if (line.GlobalRow + line.RowCount <= row)
                continue;

            int column = line.GetIndex((int)MathF.Round((x - line.LeftPadPX) / GUI.CharacterWidth), row - line.GlobalRow);
            if (shifting)
                caret.SetEndIndex(i, column);
            else
                caret.SetCaretIndex(i, column);

            break;
        }
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

        caret.RenderSelection(pageLeftPad, (int)GetScrollPX());
        
        int drawnLines = document.Lines[scrollMinVisible].GlobalRow;
        for (int lineIdx = scrollMinVisible; lineIdx < scrollMinVisible + GetMaxVisibleLines(); lineIdx++)
        {
            Line line = document.Lines[lineIdx];
            // Page numbers!
            if (!line.PDFUnrenderedCache && (line.GlobalPDFRow % Document.LinesPerPage) == 0)
            {
                string sidebar = $"{(line.GlobalPDFRow / Document.LinesPerPage) + 1}.";
                RenderHelper.Text(
                    sidebar,
                    pageLeftPad + GUI.Inch(GUI.ActionLeftPad) + GUI.TextWidth(61),
                    GUI.TextSize * drawnLines - (int)GetScrollPX(),
                    false, false, false, GUI.Syntax, true);
            }
            // Scene numbers!
            if (line.Kind == LineKind.Scene)
            {
                string sidebar = $"{line.GlobalScene}";
                RenderHelper.Text(
                    sidebar,
                    pageLeftPad + GUI.Inch(GUI.ActionLeftPad) - GUI.TextWidth(1 + sidebar.Length),
                    GUI.TextSize * drawnLines - (int)GetScrollPX(),
                    false, false, false, GUI.Foreground, false);
            }
            // Markers!
            if (line.Kind == LineKind.Note && line.IsMarker)
            {
                RenderMarker(pageLeftPad, GUI.TextSize * drawnLines - (int)GetScrollPX());
            }

            foreach (List<LineFragment> fragments in line.Content)
            {
                int xOffset = 0;

                foreach (LineFragment fragment in fragments)
                {
                    xOffset += RenderHelper.Text(
                        fragment.Content,
                        pageLeftPad + line.LeftPadPX + xOffset,
                        GUI.TextSize * drawnLines - (int)GetScrollPX(),
                        fragment.Italic, fragment.Bold, fragment.Underline,
                        line.Color,
                        fragment.Syntax);
                }
                drawnLines++;
            }
        }
        caret.RenderCaret(pageLeftPad, (int)GetScrollPX());
        RenderScrollBar();
        RenderMarkerHints();
        Raylib.EndDrawing();
        RenderHelper.NeedsRender = false;
    }

    static int ScrollBarPadding => GUI.Point(5);

    static void RenderScrollBar()
    {
        float maxScrollDistance = maxScroll + GUI.TopPad;

        int scrollBarPadding = ScrollBarPadding;
        int scrollBarArea = ScreenHeight - scrollBarPadding * 2;
        int scrollBarWidth = GUI.Point(3);
        int scrollBarHeight = (int)(ScreenHeight * scrollBarArea / (maxScrollDistance + ScreenHeight));
        int scrollBarTopY = scrollBarPadding;
        int scrollBarBottomY = ScreenHeight - scrollBarPadding - scrollBarHeight;
        int scrollBarX = ScreenWidth - scrollBarPadding - scrollBarWidth;

        float scrollPercentage = (GetScrollPX() + GUI.TopPad) / (maxScrollDistance);

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

    static void RenderMarker(int x, int y)
    {
        x += GUI.Inch(GUI.ActionLeftPad);
        y -= GUI.Point(0.5f);

        int height = GUI.TextSize;
        int heightExpand = GUI.Point(-0.5f);
        int width = GUI.Point(40);
        int inset = height;
        int padding = GUI.Point(20);

        Raylib.DrawRectangle(x - padding - width + inset, y - heightExpand, width - inset, height + heightExpand * 2, GUI.Note);
        Raylib.DrawTriangle(
            new System.Numerics.Vector2(x - padding - width, y - heightExpand),
            new System.Numerics.Vector2(x - padding - width + inset, y + height / 2),
            new System.Numerics.Vector2(x - padding - width + inset, y - heightExpand),
            GUI.Note);
        Raylib.DrawTriangle(
            new System.Numerics.Vector2(x - padding - width + inset, y + height / 2),
            new System.Numerics.Vector2(x - padding - width, y + height + heightExpand),
            new System.Numerics.Vector2(x - padding - width + inset, y + height + heightExpand),
            GUI.Note);
    }

    static void RenderMarkerHints()
    {
        float radius = GUI.FloatPoint(1.5f);
        int x = ScreenWidth - ScrollBarPadding - (int)radius;
        int padding = ScrollBarPadding + (int)radius + GUI.Point(1);
        int topY = padding;
        int bottomY = ScreenHeight - padding;

        foreach (Line line in document.Lines)
        {
            if (line.Kind != LineKind.Note || !line.IsMarker) continue;

            float factor = (float)line.GlobalRow / (document.TotalRows + ScreenHeight / GUI.TextSize - 5);

            int y = (int)MathHelpers.Lerp(topY, bottomY, factor);

            Raylib.DrawCircle(x, y, radius, Raylib.Fade(GUI.Note, 0.5f));
        }
    }
}
