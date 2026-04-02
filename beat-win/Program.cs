using beat_win.doc;
using Raylib_cs;
using Windows.Win32;
using static QuestPDF.Helpers.Colors;
using Rectangle = Raylib_cs.Rectangle;

namespace beat_win;

internal static class Program
{
    static ScrollAwareTextEditorRenderer? renderer;
    static Document? document = new();
    static Caret caret = new(0, 0, document);

    [STAThread]
    static void Main(string[] args)
    {
        Application.SetColorMode(SystemColorMode.System);

        renderer = new RaylibTextEditorRenderer();
        renderer.OnDroppedFile = file => LoadDocument(new FileDocument(file));
        renderer.OnRequestRender = () => renderer.RenderDocument(document!, caret);
        renderer.OnScrollEvent = motion => renderer.UpdateScroll(document!, motion);
        renderer.OnClickOnDocument = (x, y) => SelectPixelXY(x, y, false, renderer.ScrollMinVisible);
        renderer.OnDragOnDocument = (x, y) => SelectPixelXY(x, y, true, renderer.ScrollMinVisible);
        renderer.OnInputCharacter = input => caret.Insert(input);
        renderer.OnInputCommandCharacter = SpecialInput;
        renderer.OnEnter = () => caret.Insert("\n");
        renderer.OnTab = TabPress;
        renderer.OnBackspace = () => caret.DeleteBackspace();
        renderer.OnLeft = shifting => { if (shifting) caret.MoveEndX(-1); else caret.MoveCaretX(-1); };
        renderer.OnRight = shifting => { if (shifting) caret.MoveEndX(1); else caret.MoveCaretX(1); };
        renderer.OnUp = shifting => { if (shifting) caret.MoveEndY(-1); else caret.MoveCaretY(-1); };
        renderer.OnDown = shifting => { if (shifting) caret.MoveEndY(1); else caret.MoveCaretY(1); };

        document!.InsertMultilineAtCaret(0, 0, "No file is loaded.\n\nDrop a file onto the window to continue.\n\nChanges in this document will not and can not be saved.");

        renderer.Run();
        document.AskSave();
    }

    public static void MarkDirty()
    {
        renderer?.MarkDirty();
    }

    static void TabPress()
    {
        Line line = caret.EndLine;
        if (line.Kind == LineKind.Character || line.Kind == LineKind.Dialogue)
        {
            document!.AddLine(caret.SelectionEnd.Row + 1, "()");
            caret.SetCaretIndex(caret.SelectionEnd.Row, 1);
        }
        else if (line.Kind == LineKind.Parenthetical)
        {
            document!.AddLine(caret.SelectionEnd.Row + 1, "");
            caret.SetCaretIndex(caret.SelectionEnd.Row, 0);
        }
    }

    static void SpecialInput()
    {
        if (renderer!.IsKeyCommandCharacter('S'))
        {
            document!.Save();
        }
        if (renderer!.IsKeyCommandCharacter('N'))
        {
            FileDocument? doc = FileDocument.NewWithDialog();
            if (doc != null) LoadDocument(doc);
        }
        if (renderer!.IsKeyCommandCharacter('O'))
        {
            FileDocument? doc = FileDocument.OpenWithDialog();
            if (doc != null) LoadDocument(doc);
        }
        if (renderer!.IsKeyCommandCharacter('M'))
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
        renderer?.ResetScroll();
        MarkDirty();
        document?.AskSave();
        document = doc;
        caret = new Caret(0, 0, document);
    }

    // x and y should be in terms of page pixels
    static void SelectPixelXY(int x, int y, bool shifting, int startSearchingRow = 0)
    {
        int row = y / GUI.TextSize;

        row = Math.Clamp(row, 0, document!.TotalRows - 1);

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
}
