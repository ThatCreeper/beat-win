using Raylib_cs;
using Windows.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Rectangle = Raylib_cs.Rectangle;

namespace beat_win;

public class RaylibTextEditorRenderer : ScrollAwareTextEditorRenderer
{
    public RaylibTextEditorRenderer()
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(GUI.Inch(10), GUI.Inch(6), "(beat)");
        Raylib.EnableEventWaiting();
        SetTitleDark(true);

        GUI.Load();
        ResizeCenter(GUI.Inch(10), GUI.Inch(6));
    }

    public override void Run()
    {
        while (!Raylib.WindowShouldClose())
        {
            FileLoader();

            UpdateScroll();
            UpdateMouse();

            LineGeneralInput();
            if (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl))
            {
                OnInputCommandCharacter?.Invoke();
            }
            else
            {
                LineRowInput();
            }

            //ClampScroll();

            if (NeedsRender)
            {
                OnRequestRender!.Invoke();
            }
            else
            {
                Raylib.PollInputEvents();
            }
        }

        Raylib.CloseWindow();
    }

    void LineGeneralInput()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            OnEnter?.Invoke();
        }
        if (Raylib.IsKeyPressed(KeyboardKey.Tab))
        {
            OnTab?.Invoke();
        }

        bool shifting = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);

        if (KeyOrRepeat(KeyboardKey.Left))
        {
            OnLeft?.Invoke(shifting);
        }
        if (KeyOrRepeat(KeyboardKey.Right))
        {
            OnRight?.Invoke(shifting);
        }
        if (KeyOrRepeat(KeyboardKey.Up))
        {
            OnUp?.Invoke(shifting);
        }
        if (KeyOrRepeat(KeyboardKey.Down))
        {
            OnDown?.Invoke(shifting);
        }
    }

    void LineRowInput()
    {
        int input;
        while ((input = Raylib.GetCharPressed()) != 0)
        {
            if (input < 32 || input > 125) continue;
            OnInputCharacter?.Invoke(((char)input).ToString());
            //mouseVisible = false;
        }
        if (KeyOrRepeat(KeyboardKey.Backspace))
        {
            //mouseVisible = false;
            OnBackspace?.Invoke();
        }
    }

    void UpdateScroll()
    {
        float motion = -GUI.FloatInch(Raylib.GetMouseWheelMoveV().Y * 0.5f);
        if (motion == 0) return;
        OnScrollEvent?.Invoke(motion);
    }

    void SetTitleBarText(Document document)
    {
        Raylib.SetWindowTitle($"(beat) {document.Name()} {(document.Edited ? "(Unsaved)" : "")}");
    }

    public override int Text(string text, int x, int y, bool italic, bool bold, bool underline, Raylib_cs.Color color, bool syntax)
    {
        Raylib_cs.Font font = GUI.GetFont(italic, bold);
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

    void FileLoader()
    {
        if (Raylib.IsFileDropped())
        {
            string[] files = Raylib.GetDroppedFiles();
            if (files.Length == 0) return;
            OnDroppedFile?.Invoke(files[0]);
        }
    }
    unsafe void SetTitleDark(bool dark)
    {
        int value = dark ? 1 : 0;
        PInvoke.DwmSetWindowAttribute(
            (Windows.Win32.Foundation.HWND)Raylib.GetWindowHandle(),
            Windows.Win32.Graphics.Dwm.DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
            &value,
            sizeof(int));
    }

    void ResizeCenter(int width, int height)
    {
        var oldWidth = Raylib.GetScreenWidth();
        var oldHeight = Raylib.GetScreenHeight();
        var oldPos = Raylib.GetWindowPosition();
        Raylib.SetWindowSize(width, height);
        Raylib.SetWindowPosition(
            (int)oldPos.X + (oldWidth - width) / 2,
            (int)oldPos.Y + (oldHeight - height) / 2);
    }

    void UpdateMouse()
    {
        var pageWidth = PageWidth;
        var pageLeftPad = PageLeftPad;
        var mX = Raylib.GetMouseX() - pageLeftPad;
        var mY = Raylib.GetMouseY() + ScrollPixels;

        //if (Raylib.GetMouseDelta().LengthSquared() > 0)
        //{
        //    mouseVisible = true;
        //}

        //if (mouseVisible)
        //{
            Raylib.ShowCursor();
            if (mX >= 0 && mX < pageWidth && mY >= 0)
            {
                Raylib.SetMouseCursor(MouseCursor.IBeam);

                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    if (Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift))
                    {
                        OnDragOnDocument?.Invoke(mX, mY);
                    }
                    else
                    {
                        OnClickOnDocument?.Invoke(mX, mY);
                    }
                }
                else if (Raylib.IsMouseButtonDown(MouseButton.Left))
                {
                    OnDragOnDocument?.Invoke(mX, mY);
                }
            }
            else
            {
                Raylib.SetMouseCursor(MouseCursor.Default);
            }
        //}
        //else
        //{
        //    Raylib.HideCursor();
        //}
    }
    
    bool KeyOrRepeat(KeyboardKey key)
    {
        return Raylib.IsKeyPressed(key) || Raylib.IsKeyPressedRepeat(key);
    }

    public override bool IsKeyCommandCharacter(char key)
    {
        return Raylib.IsKeyPressed((KeyboardKey) key); // This might be right lol
    }


    public static int ScreenHeight => Raylib.GetScreenHeight();
    public static int ScreenWidth => Raylib.GetRenderWidth();
    public static int PageWidth => GUI.Inch(8.5f);
    public static int PageLeftPad => (ScreenWidth - PageWidth) / 2;
    public static int ScrollBarPadding => GUI.Point(5);

    public override void RenderDocument(Document document, Caret caret)
    {
        UpdateMaxScroll(document);
        SetTitleBarText(document);

        var screenHeight = ScreenHeight;
        var pageWidth = PageWidth;
        var pageLeftPad = PageLeftPad;

        Raylib.BeginDrawing();
        Raylib.ClearBackground(GUI.UIBackground);
        Raylib.DrawRectangle(pageLeftPad, 0, pageWidth, screenHeight, GUI.Background);

        RenderSelection(document, caret, pageLeftPad, ScrollPixels);

        int drawnLines = document.Lines[ScrollMinVisible].GlobalRow;
        for (int lineIdx = ScrollMinVisible; lineIdx < ScrollMinVisible + GetMaxVisibleLines(document); lineIdx++)
        {
            Line line = document.Lines[lineIdx];
            // Page numbers!
            if (!line.PDFUnrenderedCache && (line.GlobalPDFRow % Document.LinesPerPage) == 0)
            {
                string sidebar = $"{(line.GlobalPDFRow / Document.LinesPerPage) + 1}.";
                Text(
                    sidebar,
                    pageLeftPad + GUI.Inch(GUI.ActionLeftPad) + GUI.TextWidth(61),
                    GUI.TextSize * drawnLines - ScrollPixels,
                    false, false, false, GUI.Syntax, true);
            }
            // Scene numbers!
            if (line.Kind == LineKind.Scene)
            {
                string sidebar = $"{line.GlobalScene}";
                Text(
                    sidebar,
                    pageLeftPad + GUI.Inch(GUI.ActionLeftPad) - GUI.TextWidth(1 + sidebar.Length),
                    GUI.TextSize * drawnLines - ScrollPixels,
                    false, false, false, GUI.Foreground, false);
            }
            // Markers!
            if (line.Kind == LineKind.Note && line.IsMarker)
            {
                RenderMarker(pageLeftPad, GUI.TextSize * drawnLines - ScrollPixels);
            }

            foreach (List<LineFragment> fragments in line.Content)
            {
                int xOffset = 0;

                foreach (LineFragment fragment in fragments)
                {
                    xOffset += Text(
                        fragment.Content,
                        pageLeftPad + line.LeftPadPX + xOffset,
                        GUI.TextSize * drawnLines - ScrollPixels,
                        fragment.Italic, fragment.Bold, fragment.Underline,
                        line.Color,
                        fragment.Syntax);
                }
                drawnLines++;
            }
        }
        RenderCaret(document, caret, pageLeftPad, ScrollPixels);
        RenderScrollBar(document);
        RenderMarkerHints(document);
        RenderMenu();
        Raylib.EndDrawing();
        MarkNotDirty();
    }

    //static void ClampScroll()
    //{
    //    ScrollMinVisible = Math.Clamp(ScrollMinVisible, 0, document.Lines.Count - 1);
    //}


    void RenderScrollBar(Document document)
    {
        float maxScrollDistance = MaxScroll + GUI.TopPad;

        int scrollBarPadding = ScrollBarPadding;
        int scrollBarArea = ScreenHeight - scrollBarPadding * 2;
        int scrollBarWidth = GUI.Point(3);
        int scrollBarHeight = (int)(ScreenHeight * scrollBarArea / (maxScrollDistance + ScreenHeight));
        int scrollBarTopY = scrollBarPadding;
        int scrollBarBottomY = ScreenHeight - scrollBarPadding - scrollBarHeight;
        int scrollBarX = ScreenWidth - scrollBarPadding - scrollBarWidth;

        float scrollPercentage = (ScrollPixels + GUI.TopPad) / (maxScrollDistance);

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

    void RenderMarker(int x, int y)
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

    void RenderMarkerHints(Document document)
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

    void RenderMenu()
    {
        // TODO IMPLEMENTME
    }

    int GetMaxVisibleLines(Document document)
    {
        return Math.Min(
            (int)Math.Ceiling((float)ScreenHeight / GUI.TextSize) + 2,
            document.Lines.Count - ScrollMinVisible);
    }

    public void RenderSelection(Document document, Caret caret, int pageOffset, int scrollPixels)
    {
        if (!caret.IsSelecting) return;

        for (int i = caret.SelectionMin.Row; i <= caret.SelectionMax.Row; i++)
        {
            Line line = document.Lines[i];
            int startIndex = i == caret.SelectionMin.Row ? caret.SelectionMin.Index : 0;
            int endIndex = i == caret.SelectionMax.Row ? caret.SelectionMax.Index : line.MaxIdx;

            int lineStartIndex = 0;
            for (int j = 0; j < line.RowCount; j++)
            {
                int length = line.ContentLengths[j];
                if (startIndex > lineStartIndex + length) continue;
                if (endIndex < lineStartIndex) break;

                int left = GUI.TextWidth(Math.Max(0, startIndex - lineStartIndex));
                int right = GUI.TextWidth(Math.Min(length, endIndex - lineStartIndex));

                Raylib.DrawRectangle(
                    pageOffset + line.LeftPadPX + left,
                    (line.GlobalRow + j) * GUI.TextSize - scrollPixels,
                    right - left,
                    GUI.TextSize,
                    Raylib.Fade(GUI.Cursor, 0.5f));

                lineStartIndex += length;
            }
        }
    }

    public void RenderCaret(Document document, Caret caret, int pageLeftPad, int scrollPixels)
    {
        int row = caret.SelectionEnd.Row;
        int index = caret.SelectionEnd.Index;

        Line line = document.Lines[row];
        int rows = line.GetCursorSublineY(index);
        for (int i = 0; i < row; i++)
        {
            rows += document.Lines[i].RowCount;
        }

        int linePad = line.LeftPadPX;
        if (line.IsBlank && row != 0 && document.Lines[row - 1].DoesDialogueComeNext)
        {
            linePad = GUI.Inch(GUI.DialogueLeftPad);
        }

        Raylib.DrawRectangleRounded(
            new Raylib_cs.Rectangle(
                GUI.TextWidth(line.GetCursorCharX(index)) + pageLeftPad + linePad - GUI.Point(1),
                rows * GUI.TextSize - GUI.Point(1.5f) - scrollPixels,
                GUI.Point(1.5f),
                GUI.TextSize + GUI.Point(1)),
            0.8f,
            0,
            GUI.Cursor);
    }
}
