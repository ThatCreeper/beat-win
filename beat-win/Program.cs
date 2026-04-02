using Raylib_cs;
using Windows.Win32;
using static QuestPDF.Helpers.Colors;
using Rectangle = Raylib_cs.Rectangle;

namespace beat_win;

internal static class Program
{
    static ScrollAwareTextEditorRenderer renderer = new RaylibTextEditorRenderer();
    static Document document = new();
    static Caret caret = new(0, 0, document);

    static bool mouseVisible = true;

    [STAThread]
    static void Main(string[] args)
    {
        Application.SetColorMode(SystemColorMode.System);

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

            renderer.UpdateScroll(document);
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

            if (renderer.NeedsRender)
            {
                renderer.UpdateMaxScroll(document);
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

    public static void MarkDirty()
    {
        renderer.MarkDirty();
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
        if (Raylib.IsKeyPressed(KeyboardKey.M))
        {
            NativeWindow native = new();
            unsafe
            {
                native.AssignHandle((nint)Raylib.GetWindowHandle());
            }
            new FormSettingsMenu().ShowDialog(native);
        }
    }

    static void LoadDocument(Document doc)
    {
        renderer.ResetScroll();
        MarkDirty();
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

    // Should make me not ever have to deal with this.
    static void ClampScroll()
    {
        renderer.ScrollMinVisible = Math.Clamp(renderer.ScrollMinVisible, 0, document.Lines.Count - 1);
    }

    static void UpdateMouse()
    {
        var pageWidth = RaylibTextEditorRenderer.PageWidth;
        var pageLeftPad = RaylibTextEditorRenderer.PageLeftPad;
        var mX = Raylib.GetMouseX() - pageLeftPad;
        var mY = Raylib.GetMouseY() + (int)renderer.GetScrollPX(document);

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
                    SelectPixelXY(mX, mY, Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift), renderer.ScrollMinVisible);
                }
                else if (Raylib.IsMouseButtonDown(MouseButton.Left))
                {
                    SelectPixelXY(mX, mY, true, renderer.ScrollMinVisible);
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


    // RENDER!

    static void Render()
    {
        renderer.RenderDocument(document, caret);
    }
}
