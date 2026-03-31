using System;
using System.Collections.Generic;
using System.Text;

namespace beat_win;

public class LineMutator(Line line)
{
    public Line Line = line;

    public string RawContent
    {
        get => Line.RawContent;
        set => Line.UnsafeSetRawContent(value);
    }

    public void Rewrite(string newValue)
    {
        RawContent = newValue;
    }

    public void AddCharacter(int cursorIdx, char input)
    {
        AddString(cursorIdx, input.ToString());
    }

    public void RemoveCharacterBackwards(int cursorIdx)
    {
        if (cursorIdx == 0) return;
        RawContent = RawContent.Remove(cursorIdx - 1, 1);
    }

    public string RemoveAllAfterPosition(int cursorIdx)
    {
        string result = RawContent.Substring(cursorIdx);
        RawContent = RawContent.Remove(cursorIdx);
        return result;
    }

    public void AddString(int cursorIdx, string add)
    {
        RawContent = RawContent.Insert(cursorIdx, add);
    }

    public int AddStringEnd(string add)
    {
        int idx = Line.MaxIdx;
        AddString(idx, add);
        return idx;
    }
}
