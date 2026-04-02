namespace beat_win.doc;

public enum LineKind
{
    Action,
    PageBreak,
    Character,
    Parenthetical,
    Dialogue,
    Preamble,
    Nonexistant,
    Note,
    Scene,
    Center,
    Right
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
    public int GlobalScene = 0;
    public bool PDFUnrenderedCache = false;
    public LineKind Kind = LineKind.Action;
    string rawContent;

    public string RawContent => rawContent;

    public bool IsBlank => rawContent.Length == 0;
    public int RowCount => Content.Count;
    public int PDFRowCount => PDFUnrenderedCache ? 0 : RowCount;
    public int MaxIdx => Content.Sum(c => c.Sum((LineFragment f) => f.Content.Length));
    public bool DoesDialogueComeNext => Kind == LineKind.Character || Kind == LineKind.Parenthetical || Kind == LineKind.Dialogue;


    public float LeftPad = GUI.ActionLeftPad;
    public float RightPad = GUI.ActionRightPad;
    public int LeftPadPX => GUI.Inch(LeftPad);
    public int RightPadPX => GUI.Inch(RightPad);
    public Raylib_cs.Color Color = GUI.Foreground;

    public bool IsMarker = false;

    public Line(string content = "")
    {
        Content = [[new LineFragment(content)]];
        ContentLengths = [content.Length];
        rawContent = content;
    }

    public void InternalRecombobulateDontCall(LineKind previousKind)
    {
        Kind = DetermineKind(previousKind);
        PerformKindModifications();
        SetPaddingAndColor();
        WrapAndStyle();
    }

    private LineKind DetermineKind(LineKind previousKind)
    {
        // Forces
        if (rawContent.StartsWith('!'))
            return LineKind.Action;
        if (rawContent.StartsWith("FADE IN:"))
            return LineKind.Action;
        if (rawContent.StartsWith('>'))
            return rawContent.EndsWith('<') ? LineKind.Center : LineKind.Right;

        // Otherwise
        if (rawContent.StartsWith("[[") && rawContent.EndsWith("]]"))
        {
            IsMarker = rawContent.StartsWith("[[marker ")
                || rawContent.StartsWith("[[marker:");
            return LineKind.Note;
        }
        if (rawContent.EndsWith(" TO:") || rawContent.EndsWith(" OUT."))
        {
            return LineKind.Right;
        }
        if (previousKind == LineKind.Nonexistant && rawContent.Contains(':'))
            return LineKind.Preamble;
        if (previousKind == LineKind.Preamble && !IsBlank)
            return LineKind.Preamble;
        if (rawContent == "===")
            return LineKind.PageBreak;
        if (rawContent.StartsWith('('))
            return LineKind.Parenthetical;
        if ((previousKind == LineKind.Character ||
            previousKind == LineKind.Parenthetical ||
            previousKind == LineKind.Dialogue) && !IsBlank)
            return LineKind.Dialogue;
        if (rawContent.Length >= 4 && (rawContent[3] == '.' || rawContent[3] == ' ' || rawContent[3] == '/') &&
            (rawContent.StartsWith("INT", StringComparison.InvariantCultureIgnoreCase) || rawContent.StartsWith("EXT", StringComparison.InvariantCultureIgnoreCase) ||
            rawContent.StartsWith("EST", StringComparison.InvariantCultureIgnoreCase) || rawContent.StartsWith("I/E", StringComparison.InvariantCultureIgnoreCase)))
            return LineKind.Scene;
        if (rawContent.Length >= 2 && IsRawContentUpper())
            return LineKind.Character;
        return LineKind.Action;
    }

    private void PerformKindModifications()
    {
        if (Kind == LineKind.Scene)
        {
            rawContent = rawContent.ToUpper();
        }
    }

    private void SetPaddingAndColor()
    {
        LeftPad = Kind switch
        {
            LineKind.Character => GUI.CharacterLeftPad,
            LineKind.Parenthetical => GUI.ParentheticalLeftPad,
            LineKind.Dialogue => GUI.DialogueLeftPad,
            LineKind.Preamble => GUI.DialogueLeftPad,
            LineKind.Nonexistant => 0,
            // These break for multiline wrapped things so just don't do that thx
            LineKind.Right => 8.5f - GUI.ActionRightPad - rawContent.Length * GUI.CharacterWidthInch,
            LineKind.Center => (8.5f - rawContent.Length * GUI.CharacterWidthInch) / 2,
            _ => GUI.ActionLeftPad
        };
        RightPad = Kind switch
        {
            LineKind.Parenthetical => GUI.ParentheticalRightPad,
            LineKind.Dialogue => GUI.DialogueRightPad,
            LineKind.Nonexistant => 0,
            LineKind.Right => -1000,
            LineKind.Center => -1000,
            _ => GUI.ActionRightPad
        };
        Color = Kind switch
        {
            LineKind.Preamble => GUI.Syntax,
            LineKind.Nonexistant => Raylib_cs.Color.Red,
            LineKind.Note => GUI.Note,
            _ => GUI.Foreground
        };
    }

    // Length = 0 returns true
    private bool IsRawContentUpper()
    {
        bool HasAlphanumeric = false;
        for (int i = 0; i < rawContent.Length; i++)
        {
            if (Char.IsLower(rawContent[i]))
                return false;
            HasAlphanumeric = HasAlphanumeric || Char.IsLetter(rawContent[i]);
            if (rawContent[i] == '*' || rawContent[i] == '_')
                return false;
        }
        return true;
    }

    private void WrapAndStyle()
    {
        Content.Clear();
        ContentLengths.Clear();

        int lineLength = (int)((8.5 - LeftPad - RightPad) / GUI.CharacterWidthInch);
        int start = 0;
        int x = 0;

        List<LineFragment> fragments = new();
        bool bold = false;
        bool italic = false;
        bool underline = false;

        if (Kind == LineKind.Scene)
        {
            bold = true;
        }
        else if (Kind == LineKind.Note)
        {
            italic = true;
        }

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
            else if (rawContent[i] == '\\')
            {
                PushFragment(i, false);
                i++;
                PushFragment(i, true);
            }
            else if (i == 0 && rawContent[i] == '!')
            {
                PushFragment(i + 1, true);
            }
            else if (i == 0 && rawContent[i] == '>')
            {
                PushFragment(i + 1, true);
            }
            else if (Kind == LineKind.Center && i == rawContent.Length - 1 && rawContent[i] == '<')
            {
                PushFragment(i, false);
                PushFragment(i + 1, true);
            }
            else if (Kind == LineKind.Character && i == rawContent.Length - 1 && rawContent[i] == '^')
            {
                PushFragment(i, false);
                PushFragment(i + 1, true);
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
        idx += Math.Clamp(x, 0, ContentLengths[y]);
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
        if (Kind == LineKind.Preamble)
            return true;
        if (Kind == LineKind.Note)
            return true;
        if (priorUnrendered && IsBlank)
            return true;

        return false;
    }
}
