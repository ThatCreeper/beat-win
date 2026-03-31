namespace beat_win;

public class Line
{
    public List<string> Content;

    public Line(string content = "")
    {
        Content = [content];
        Recombobulate();
    }

    public void Recombobulate(string? content = null)
    {
        content = content ?? RawContent;
        Wrap(content);
    }

    private void Wrap(string content)
    {
        Content.Clear();

        int lineLength = (int)((8.5 - LeftPad - RightPad) * 10);
        int start = 0;
        int x = 0;
        for (int i = 0; i < content.Length; i++)
        {
            x++;
            if (x <= lineLength) continue;
            if (content[i] == ' ') continue;
            bool foundSpace = false;
            for (int j = i - 1; j >= start; j--)
            {
                if (content[j] != ' ') continue;
                foundSpace = true;
                j++;
                Content.Add(content.Substring(start, j - start));
                start = j;
                x = i - j + 1;
                break;
            }
            if (!foundSpace)
            {
                Content.Add(content.Substring(start, i - start));
                start = i;
                x = 1;
            }
        }

        Content.Add(content.Substring(start));
    }

    public string RawContent => String.Join("", Content);
    public bool IsBlank => Content.Count == 1 && Content[0].Length == 0;
    public int RowCount => Content.Count;
    public int MaxIdx => Content.Sum(c => c.Length);

    public void AddCharacter(int cursorIdx, char input)
    {
        AddString(cursorIdx, input.ToString());
    }

    public void RemoveCharacterBackwards(int cursorIdx)
    {
        if (cursorIdx == 0) return;
        Recombobulate(RawContent.Remove(cursorIdx - 1, 1));
    }

    public string RemoveAllAfterPosition(int cursorIdx)
    {
        string result = RawContent.Substring(cursorIdx);
        Recombobulate(RawContent.Remove(cursorIdx));
        return result;
    }

    // I don't like this code dup, but it makes the API nice.
    public int GetCursorCharX(int cursorIdx)
    {
        for (int i = 0; i < Content.Count; i++)
        {
            if (cursorIdx <= Content[i].Length)
            {
                return cursorIdx;
            }
            else
            {
                cursorIdx -= Content[i].Length;
            }
        }
        return 0;
    }

    public int GetCursorSublineY(int cursorIdx)
    {
        for (int i = 0; i < Content.Count; i++)
        {
            if (cursorIdx <= Content[i].Length)
            {
                return i;
            }
            else
            {
                cursorIdx -= Content[i].Length;
            }
        }
        return 0;
    }

    public int GetIndex(int x, int y)
    {
        int idx = 0;
        for (int i = 0; i < y; i++)
        {
            idx += Content[i].Length;
        }
        idx += Math.Min(x, Content[y].Length);
        return idx;
    }

    public void AddString(int cursorIdx, string add)
    {
        Recombobulate(RawContent.Insert(cursorIdx, add));
    }

    public int AddStringEnd(string add)
    {
        int idx = MaxIdx;
        AddString(idx, add);
        return idx;
    }

    public float LeftPad = GUI.ActionLeftPad;
    public float RightPad = GUI.ActionRightPad;
    public int LeftPadPX => GUI.Inch(LeftPad);
    public int RightPadPX => GUI.Inch(RightPad);
}
