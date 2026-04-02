using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace beat_win.doc;

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

    public bool IsSelecting => !SelectionStart.Equals(SelectionEnd);

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
            SelectionEnd.Index += EndLine.MaxIdx + 1;
        }
        while (SelectionEnd.Index > EndLine.MaxIdx)
        {
            if (SelectionEnd.Row == document.Lines.Count - 1)
            {
                SelectionEnd.Index = EndLine.MaxIdx;
                break;
            }
            SelectionEnd.Index -= EndLine.MaxIdx + 1;
            SelectionEnd.Row++;
        }
        upDownXPos = EndLine.GetCursorCharX(SelectionEnd.Index);
        Program.MarkDirty();
    }

    public void MoveEndY(int y)
    {
        int modifiedSubrow = EndLine.GetCursorSublineY(SelectionEnd.Index) + y;
        while (modifiedSubrow < 0)
        {
            if (SelectionEnd.Row == 0)
            {
                SelectionEnd.Index = 0;
                Program.MarkDirty();
                return;
            }
            SelectionEnd.Row--;
            modifiedSubrow += EndLine.RowCount;
        }
        while (modifiedSubrow >= EndLine.RowCount)
        {
            if (SelectionEnd.Row == document.Lines.Count - 1)
            {
                SelectionEnd.Index = EndLine.MaxIdx;
                Program.MarkDirty();
                return;
            }
            modifiedSubrow -= EndLine.RowCount;
            SelectionEnd.Row++;
        }
        SelectionEnd.Index = EndLine.GetIndex(upDownXPos, modifiedSubrow);
        Program.MarkDirty();
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
        Program.MarkDirty();
    }

    public void SetCaretIndex(int row, int index)
    {
        SetEndIndex(row, index);
        SelectionStart = SelectionEnd;
    }


    public void Insert(string text)
    {
        if (IsSelecting) DeleteBackspace();

        int rows;
        SelectionEnd.Index = document.InsertMultilineAtCaret(SelectionEnd.Row, SelectionEnd.Index, text, out rows);
        SelectionEnd.Row += rows;
        SelectionStart = SelectionEnd;
        upDownXPos = EndLine.GetCursorCharX(SelectionEnd.Index);
    }

    public void Insert(char text)
    {
        Insert(text.ToString());
    }

    // Backspace
    public string DeleteBackspace()
    {
        if (IsSelecting)
        {
            if (SelectionMin.Row == SelectionMax.Row)
            {
                string removed = document.Alter(SelectionMin.Row, mut => mut.RemoveSomeAfterPosition(SelectionMin.Index, SelectionMax.Index - SelectionMin.Index));
                SetCaretIndex(SelectionMin.Row, SelectionMin.Index);
                return removed;
            }
            else
            {
                // First row
                string removed = document.Alter(SelectionMin.Row, mut => mut.RemoveAllAfterPosition(SelectionMin.Index));
                // Middle rows
                SelectionPosition max = SelectionMax;
                for (int i = 0; i < max.Row - SelectionMin.Row - 1; i++)
                {
                    removed += "\n" + document.Remove(SelectionMin.Row + 1);
                }
                // Last row
                removed += "\n" + document.Alter(SelectionMin.Row + 1, mut => mut.RemoveSomeAfterPosition(0, max.Index));
                // Combine
                SetCaretIndex(SelectionMin.Row + 1, 0);
                DeleteBackspace();
                return removed;
            }
        }
        else
        {
            if (SelectionEnd.Index == 0)
            {
                if (SelectionEnd.Row != 0)
                {
                    string removed = document.Remove(SelectionEnd.Row);
                    MoveCaretX(-1);
                    document.InsertMultilineAtCaret(SelectionEnd.Row, SelectionEnd.Index, removed);
                    return "\n";
                }
                else
                {
                    return "";
                }
            }
            else
            {
                string removed = document.Alter(SelectionEnd.Row, mut => mut.RemoveCharacterBackwards(SelectionEnd.Index).ToString());
                MoveCaretX(-1);
                return removed;
            }
        }
    }
}
