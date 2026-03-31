namespace beat_win;

public enum LineKind
{
    Action,
    PageBreak
}

public class Line
{

    public List<string> Content;
    public int GlobalRow = 0;
    public int GlobalPDFRow = 0;
    public bool PDFUnrenderedCache = false;
    public LineKind Kind = LineKind.Action;
    string rawContent;

    public string RawContent => rawContent;

    public bool IsBlank => Content.Count == 1 && Content[0].Length == 0;
    public int RowCount => Content.Count;
    public int PDFRowCount => PDFUnrenderedCache ? 0 : RowCount;
    public int MaxIdx => Content.Sum(c => c.Length);

    public float LeftPad = GUI.ActionLeftPad;
    public float RightPad = GUI.ActionRightPad;
    public int LeftPadPX => GUI.Inch(LeftPad);
    public int RightPadPX => GUI.Inch(RightPad);

    public Line(string content = "")
    {
        Content = [content];
        rawContent = content;
    }

    public void InternalRecombobulateDontCall()
    {
        Kind = DetermineKind();
        Wrap();
    }

    private LineKind DetermineKind()
    {
        if (rawContent == "===")
            return LineKind.PageBreak;
        return LineKind.Action;
    }

    private void Wrap()
    {
        Content.Clear();

        int lineLength = (int)((8.5 - LeftPad - RightPad) * 10);
        int start = 0;
        int x = 0;
        for (int i = 0; i < rawContent.Length; i++)
        {
            x++;
            if (x <= lineLength) continue;
            if (rawContent[i] == ' ') continue;
            bool foundSpace = false;
            for (int j = i - 1; j >= start; j--)
            {
                if (rawContent[j] != ' ') continue;
                foundSpace = true;
                j++;
                Content.Add(rawContent.Substring(start, j - start));
                start = j;
                x = i - j + 1;
                break;
            }
            if (!foundSpace)
            {
                Content.Add(rawContent.Substring(start, i - start));
                start = i;
                x = 1;
            }
        }

        Content.Add(rawContent.Substring(start));
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

    public void UnsafeSetRawContent(string newValue)
    {
        rawContent = newValue;
    }

    public bool IsUnrenderedPDF(bool priorUnrendered)
    {
        if (Kind == LineKind.PageBreak)
            return true;
        if (priorUnrendered && IsBlank)
            return true;

        return false;
    }
}
