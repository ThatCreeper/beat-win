using beat_win.doc;
using beat_win.forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace beat_win;

public class WindowsTextEditorRenderer : ScrollAwareTextEditorRenderer
{
    public FormWindowsEditor form;
    public char CommandCharacter;
    public PaintEventArgs? PaintEvent;
    public Rectangle DrawRect = new Rectangle();
    PrivateFontCollection privateCollection;
    FontFamily family;

    Font fontN;
    Font fontB;
    Font fontI;
    Font fontBI;
    Font fontU;
    Font fontBU;
    Font fontIU;
    Font fontBIU;

    public WindowsTextEditorRenderer()
    {
        form = new FormWindowsEditor(this);
        //form.Show();

        //GUI.Update(form.DeviceDpi / 72.0f);

        privateCollection = new PrivateFontCollection();
        privateCollection.AddFontFile("Courier Prime.ttf");
        family = privateCollection.Families[0];
        
        fontN = new Font(family, GUI.TextSize, FontStyle.Regular, GraphicsUnit.Pixel);
        fontB = new Font(family, GUI.TextSize, FontStyle.Bold, GraphicsUnit.Pixel);
        fontI = new Font(family, GUI.TextSize, FontStyle.Italic, GraphicsUnit.Pixel);
        fontBI = new Font(family, GUI.TextSize, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Pixel);
        fontU = new Font(family, GUI.TextSize, FontStyle.Underline, GraphicsUnit.Pixel);
        fontBU = new Font(family, GUI.TextSize, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Pixel);
        fontIU = new Font(family, GUI.TextSize, FontStyle.Italic | FontStyle.Underline, GraphicsUnit.Pixel);
        fontBIU = new Font(family, GUI.TextSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline, GraphicsUnit.Pixel);

        GUI.CharacterWidth = TextRenderer.MeasureText("m", fontN, Size.Empty, TextFormatFlags.NoPadding).Width;
    }

    public override IWin32Window GetWindowHandle()
    {
        return form;
    }

    public override bool IsKeyCommandCharacter(char key)
    {
        return key == CommandCharacter;
    }

    int GetMaxVisibleLines(Document document, int height)
    {
        return Math.Min(
            (int)Math.Ceiling((float)height / GUI.TextSize) + 2,
            document.Lines.Count - ScrollMinVisible);
    }

    public override void RenderDocument(Document document, Caret caret)
    {
        try
        {
            if (PaintEvent == null) return;
            var p = PaintEvent;
            var bounds = DrawRect;

            UpdateMaxScroll(document);

            //PInvoke.HideCaret((HWND)form.Handle);

            int pageWidth = GUI.Inch(8.5f);
            int pageLeftPad = (bounds.Width - pageWidth) / 2;

            using SolidBrush bg = new(Color.SlateGray);
            using SolidBrush pageBg = new(Color.White);

            p.Graphics.FillRectangle(bg, bounds);
            p.Graphics.FillRectangle(pageBg, new Rectangle(pageLeftPad, 0, pageWidth, bounds.Height));

            int drawnLines = document.Lines[ScrollMinVisible].GlobalRow;
            for (int lineIdx = ScrollMinVisible; lineIdx < ScrollMinVisible + GetMaxVisibleLines(document, bounds.Height); lineIdx++)
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
                        false, false, false, GUI.Syntax, true, false);
                }
                // Scene numbers!
                if (line.Kind == LineKind.Scene)
                {
                    string sidebar = $"{line.GlobalScene}";
                    Text(
                        sidebar,
                        pageLeftPad + GUI.Inch(GUI.ActionLeftPad) - GUI.TextWidth(1 + sidebar.Length),
                        GUI.TextSize * drawnLines - ScrollPixels,
                        false, false, false, GUI.Foreground, false, false);
                }
                // Markers!
                if (line.Kind == LineKind.Note && line.IsMarker)
                {
                    //RenderMarker(pageLeftPad, GUI.TextSize * drawnLines - ScrollPixels);
                }

                bool highlighted = caret.IsSelecting && caret.SelectionMin.Row <= lineIdx
                    && caret.SelectionMax.Row >= lineIdx;
                int highlightStart = highlighted ? (caret.SelectionMin.Row == lineIdx ? caret.SelectionMin.Index : 0) : 0;
                int highlightEnd = highlighted ? (caret.SelectionMax.Row == lineIdx ? caret.SelectionMax.Index : line.MaxIdx) : 0;

                int indexInc = 0;

                foreach (List<LineFragment> fragments in line.Content)
                {
                    int xOffset = 0;

                    foreach (LineFragment fragment in fragments)
                    {
                        var aStart = Math.Clamp(highlightStart - indexInc, 0, fragment.Content.Length);
                        var aEnd = Math.Clamp(highlightEnd - indexInc, 0, fragment.Content.Length);
                        xOffset += Text(
                            fragment.Content.Substring(0, aStart),
                            pageLeftPad + line.LeftPadPX + xOffset,
                            GUI.TextSize * drawnLines - ScrollPixels,
                            fragment.Italic, fragment.Bold, fragment.Underline,
                            line.Color,
                            fragment.Syntax,
                            false);
                        xOffset += Text(
                            fragment.Content.Substring(
                                aStart,
                                Math.Max(0, aEnd - aStart)
                                //Math.Clamp(highlightEnd - highlightStart - indexInc, 0, fragment.Content.Length - aStart)),
                                ),
                            pageLeftPad + line.LeftPadPX + xOffset,
                            GUI.TextSize * drawnLines - ScrollPixels,
                            fragment.Italic, fragment.Bold, fragment.Underline,
                            line.Color,
                            fragment.Syntax,
                            true);
                        xOffset += Text(
                            fragment.Content.Substring(aEnd),
                            pageLeftPad + line.LeftPadPX + xOffset,
                            GUI.TextSize * drawnLines - ScrollPixels,
                            fragment.Italic, fragment.Bold, fragment.Underline,
                            line.Color,
                            fragment.Syntax,
                            false);
                        indexInc += fragment.Content.Length;
                    }
                    drawnLines++;
                }
            }

            form.wordCount.Text = $"{document.Lines.Sum(l => l.RawContent.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)} words";
            form.pageCount.Text = $"{MathF.Ceiling(document.TotalPDFRows / (float)Document.LinesPerPage)} pages";

            RenderCaret(document, caret, pageLeftPad);
            //PInvoke.ShowCaret((HWND)form.Handle);

            MarkNotDirty();
        }
        catch (Exception e)
        {
            Debugger.Break();
        }
    }

    void RenderCaret(Document document, Caret caret, int pageLeftPad)
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

        PInvoke.SetCaretPos(
            GUI.TextWidth(line.GetCursorCharX(index)) + pageLeftPad + linePad,
            rows * GUI.TextSize - ScrollPixels);
    }

    public override void Run()
    {
        Application.Run(form);
    }

    public int Text(string text, int x, int y, bool italic, bool bold, bool underline, Raylib_cs.Color color, bool syntax, bool highlighted)
    {
        if (text.Length == 0) return 0;
        if (syntax) color = GUI.Syntax;
        Color gdiColor = Color.FromArgb(color.A, 255 - color.R, 255 - color.G, 255 - color.B);
        Font font = fontN;
        if (italic && bold && underline)
            font = fontBIU;
        else if (italic && underline)
            font = fontIU;
        else if (bold && underline)
            font = fontBU;
        else if (italic && bold)
            font = fontBI;
        else if (italic)
            font = fontI;
        else if (bold)
            font = fontB;
        
        // Makes me feel bad.
        // Also terrible for performance lol.
        //for (int i = 0; i < text.Length; i++)
        //{
            TextRenderer.DrawText(
                PaintEvent!,
                //text.AsSpan().Slice(i, 1),
                text,
                font,
                new Point(x, y),
                gdiColor,
                highlighted ? Color.LightGray : Color.Transparent,
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
        //x += (int)GUI.CharacterWidth;
        //}
        int wid = GUI.TextWidth(text);
        return wid;
    }

    public override void MarkDirty()
    {
        base.MarkDirty();
        form.InvalidateEditor();
    }

    public void Click(int x, int y, int width)
    {
        int pageWidth = GUI.Inch(8.5f);
        int pageLeftPad = (width - pageWidth) / 2;

        x -= pageLeftPad;
        y += ScrollPixels;

        if (y >= 0 && x >= 0 && x < pageWidth)
            OnClickOnDocument?.Invoke(x, y);
    }

    public void Drag(int x, int y, int width)
    {
        int pageWidth = GUI.Inch(8.5f);
        int pageLeftPad = (width - pageWidth) / 2;

        x -= pageLeftPad;
        y += ScrollPixels;

        if (y >= 0 && x >= 0 && x < pageWidth)
            OnDragOnDocument?.Invoke(x, y);
    }
}
