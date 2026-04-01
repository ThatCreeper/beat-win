using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace beat_win;

public struct SelectionPosition
{
    public int Row;
    public int Index;

    public readonly bool Equals(SelectionPosition other)
    {
        return Row == other.Row && Index == other.Index;
    }
}

public class Caret
{
    public SelectionPosition SelectionStart;
    public SelectionPosition SelectionEnd;

    int upDownXPos = 0;
    Document document;

    public SelectionPosition SelectionMin =>
        SelectionStart.Row == SelectionEnd.Row
            ? (SelectionStart.Index < SelectionEnd.Index ? SelectionStart : SelectionEnd)
            : (SelectionStart.Row < SelectionEnd.Row ? SelectionStart : SelectionEnd);
    public SelectionPosition SelectionMax =>
        SelectionStart.Row == SelectionEnd.Row
            ? (SelectionStart.Index > SelectionEnd.Index ? SelectionStart : SelectionEnd)
            : (SelectionStart.Row > SelectionEnd.Row ? SelectionStart : SelectionEnd);
    public Line EndLine => document.Lines[SelectionEnd.Row];

    public Caret(int row, int index, Document document)
    {
        this.document = document;
        SetCaretIndex(row, index);
    }

    public void MoveEndX(int x)
    {
        SelectionEnd.Index += x;
        while (SelectionEnd.Index < 0)
        {
            if (SelectionEnd.Row == 0)
            {
                SelectionEnd.Index = 0;
                break;
            }
            SelectionEnd.Row--;
            SelectionEnd.Index += EndLine.MaxIdx;
        }
        while (SelectionEnd.Index > EndLine.MaxIdx)
        {
            if (SelectionEnd.Row == document.Lines.Count - 1)
            {
                SelectionEnd.Index = EndLine.MaxIdx;
                break;
            }
            SelectionEnd.Index -= EndLine.MaxIdx;
            SelectionEnd.Row++;
        }
        upDownXPos = EndLine.GetCursorCharX(SelectionEnd.Index);
    }

    public void MoveEndY(int y)
    {
        int modifiedSubrow = EndLine.GetCursorSublineY(SelectionEnd.Index) + y;
        while (modifiedSubrow < 0)
        {
            if (SelectionEnd.Row == 0)
            {
                SelectionEnd.Index = 0;
                return;
            }
            SelectionEnd.Row--;
            modifiedSubrow += EndLine.RowCount;
        }
        while (modifiedSubrow >= EndLine.RowCount)
        {
            if (SelectionEnd.Row == document.Lines.Count - 1)
            {
                SelectionEnd.Index = 0;
                return;
            }
            modifiedSubrow -= EndLine.RowCount;
            SelectionEnd.Row++;
        }
        SelectionEnd.Index = EndLine.GetIndex(upDownXPos, modifiedSubrow);
    }

    public void MoveCaretX(int x)
    {
        MoveEndX(x);
        SelectionStart = SelectionEnd;
    }

    public void MoveCaretY(int y)
    {
        MoveEndY(y);
        SelectionStart = SelectionEnd;
    }

    public void SetEndIndex(int row, int index)
    {
        SelectionEnd.Row = row;
        SelectionEnd.Index = index;
        upDownXPos = EndLine.GetCursorCharX(index);
    }

    public void SetEndPos(int x, int y)
    {
        throw new NotImplementedException();
    }

    public void SetCaretIndex(int row, int index)
    {
        SetEndIndex(row, index);
        SelectionStart = SelectionEnd;
    }

    public void SetCaretPos(int x, int y)
    {
        SetEndPos(x, y);
        SelectionStart = SelectionEnd;
    }


    public void Insert(string text)
    {
        throw new NotImplementedException();
    }

    public void Insert(char text)
    {
        throw new NotImplementedException();
    }

    // Backspace
    public void DeleteBackspace()
    {
        throw new NotImplementedException();
    }


    // RENDER !

    public void RenderSelection()
    {
        if (SelectionStart.Equals(SelectionEnd)) return;
        throw new NotImplementedException();
    }

    public void RenderCaret(int pageLeftPad, int scrollPixels)
    {
        int row = SelectionEnd.Row;
        int index = SelectionEnd.Index;

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
