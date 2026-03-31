namespace beat_win;

public enum LineKind
{
    Action,
    PageBreak,
    Character,
    Parenthetical,
    Dialogue
}

public struct LineFragment(string content)
{
    public string Content = content;
    public bool Bold = false;
    public bool Italic = false;
    public bool Underline = false;
    public bool Syntax = false;
}

public class Line
{

    public List<List<LineFragment>> Content;
    public List<int> ContentLengths;
    public int GlobalRow = 0;
    public int GlobalPDFRow = 0;
    public bool PDFUnrenderedCache = false;
    public LineKind Kind = LineKind.Action;
    string rawContent;

    public string RawContent => rawContent;

    public bool IsBlank => rawContent.Length == 0;
    public int RowCount => Content.Count;
    public int PDFRowCount => PDFUnrenderedCache ? 0 : RowCount;
    public int MaxIdx => Content.Sum(c => c.Sum((LineFragment f) => f.Content.Length));

    public float LeftPad = GUI.ActionLeftPad;
    public float RightPad = GUI.ActionRightPad;
    public int LeftPadPX => GUI.Inch(LeftPad);
    public int RightPadPX => GUI.Inch(RightPad);

    public Line(string content = "")
    {
        Content = [[new LineFragment(content)]];
        ContentLengths = [content.Length];
        rawContent = content;
    }

    public void InternalRecombobulateDontCall(LineKind previousKind)
    {
        Kind = DetermineKind(previousKind);
        SetPadding();
        WrapAndStyle();
    }

    private LineKind DetermineKind(LineKind previousKind)
    {
        if (rawContent == "===")
            return LineKind.PageBreak;
        if (rawContent.StartsWith('('))
            return LineKind.Parenthetical;
        if ((previousKind == LineKind.Character ||
            previousKind == LineKind.Parenthetical ||
            previousKind == LineKind.Dialogue) && !IsBlank)
            return LineKind.Dialogue;
        if (rawContent.Length >= 2 && IsRawContentUpper())
            return LineKind.Character;
        return LineKind.Action;
    }

    private void SetPadding()
    {
        LeftPad = Kind switch
        {
            LineKind.Action => GUI.ActionLeftPad,
            LineKind.PageBreak => GUI.ActionLeftPad,
            LineKind.Character => GUI.CharacterLeftPad,
            LineKind.Parenthetical => GUI.ParentheticalLeftPad,
            LineKind.Dialogue => GUI.DialogueLeftPad,
        };
        RightPad = Kind switch
        {
            LineKind.Action => GUI.ActionRightPad,
            LineKind.PageBreak => GUI.ActionRightPad,
            LineKind.Character => GUI.ActionRightPad,
            LineKind.Parenthetical => GUI.ParentheticalRightPad,
            LineKind.Dialogue => GUI.DialogueRightPad,
        };
    }

    // Length = 0 returns true
    private bool IsRawContentUpper()
    {
        for (int i = 0; i < rawContent.Length; i++)
        {
            if (!Char.IsUpper(rawContent[i]))
                return false;
        }
        return true;
    }

    private void WrapAndStyle()
    {
        Content.Clear();
        ContentLengths.Clear();

        int lineLength = (int)((8.5 - LeftPad - RightPad) * 10);
        int start = 0;
        int x = 0;

        List<LineFragment> fragments = new();
        bool bold = false;
        bool italic = false;
        bool underline = false;

        void PushFragment(int end, bool syntax)
        {
            if (start == end) return;
            fragments.Add(new LineFragment
            {
                Content = rawContent.Substring(start, end - start),
                Bold = bold,
                Italic = italic,
                Underline = underline,
                Syntax = syntax
            });
            start = end;
        }
        void PushLine()
        {
            Content.Add(fragments);
            ContentLengths.Add(fragments.Sum(f => f.Content.Length));
            fragments = new();
        }

        char Peek(int iBefore)
        {
            if (++iBefore >= rawContent.Length) return '\0';
            return rawContent[iBefore];
        }

        for (int i = 0; i < rawContent.Length; i++)
        {
            x++;
            if (rawContent[i] == '*')
            {
                x--;

                PushFragment(i, false);
                bool nextBold = bold;
                bool nextItalic = italic;
                if (Peek(i) == '*')
                {
                    nextBold = !bold;
                    i++;
                }
                else
                {
                    nextItalic = !italic;
                }

                // Make it even on both sides.
                // I don't like this hack but it works.
                if (nextBold) bold = nextBold;
                if (nextItalic) italic = nextItalic;
                PushFragment(i + 1, true);
                bold = nextBold;
                italic = nextItalic;
            }
            else if (rawContent[i] == '_')
            {
                x--;

                PushFragment(i, false);
                bool nextUnderline = !underline;
                if (nextUnderline) underline = nextUnderline;
                PushFragment(i + 1, true);
                underline = nextUnderline;
            }

            if (x <= lineLength) continue;
            if (rawContent[i] == ' ') continue;
            bool foundSpace = false;
            for (int j = i - 1; j >= start; j--)
            {
                if (rawContent[j] != ' ') continue;
                foundSpace = true;
                j++;
                PushFragment(j, false);
                PushLine();
                start = j;
                x = i - j + 1;
                break;
            }
            if (!foundSpace)
            {
                PushFragment(i, false);
                PushLine();
                start = i;
                x = 1;
            }
        }

        PushFragment(rawContent.Length, false);
        PushLine();
    }


    // I don't like this code dup, but it makes the API nice.
    public int GetCursorCharX(int cursorIdx)
    {
        for (int i = 0; i < Content.Count; i++)
        {
            if (cursorIdx <= ContentLengths[i])
            {
                return cursorIdx;
            }
            else
            {
                cursorIdx -= ContentLengths[i];
            }
        }
        return 0;
    }

    public int GetCursorSublineY(int cursorIdx)
    {
        for (int i = 0; i < Content.Count; i++)
        {
            if (cursorIdx <= ContentLengths[i])
            {
                return i;
            }
            else
            {
                cursorIdx -= ContentLengths[i];
            }
        }
        return 0;
    }

    public int GetIndex(int x, int y)
    {
        int idx = 0;
        for (int i = 0; i < y; i++)
        {
            idx += ContentLengths[i];
        }
        idx += Math.Min(x, ContentLengths[y]);
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
