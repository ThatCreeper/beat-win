namespace beat_win;

public class Document
{
    public IReadOnlyList<Line> Lines;
    public int TotalRows = 0;
    public int TotalPDFRows = 0;
    public int TotalScenes = 0;

    List<Line> lines;

    public const int LinesPerPage = 55;

    public Document()
    {
        lines = [];
        Lines = lines;

        AddLine(0, "");
    }

    public Line AddLine(int index, string content = "")
    {
        Line line = new(content);
        if (index == 0)
        {
            line.GlobalRow = 0;
            line.GlobalPDFRow = 0;
            line.GlobalScene = 0;
            line.PDFUnrenderedCache = line.IsUnrenderedPDF(true);
        }
        else
        {
            line.GlobalRow = lines[index - 1].GlobalRow + lines[index - 1].RowCount;
            line.GlobalPDFRow = lines[index - 1].GlobalPDFRow + lines[index - 1].PDFRowCount;
            line.GlobalScene = lines[index - 1].GlobalScene;
        }
        lines.Insert(index, line);
        Recombobulate(index);
        return line;
    }

    // Reticulating splines
    void Recombobulate(int index)
    {
        InternallyRecombobulate(index);
        bool priorUnrendered = index == 0 ? true : lines[index - 1].PDFUnrenderedCache;
        priorUnrendered = lines[index].PDFUnrenderedCache = lines[index].IsUnrenderedPDF(priorUnrendered);
        if (index == 0)
        {
            TotalRows = 0;
            TotalPDFRows = 0;
            TotalScenes = 0;
        }
        else
        {
            TotalRows = lines[index - 1].GlobalRow + lines[index - 1].RowCount;
            TotalPDFRows = lines[index - 1].GlobalPDFRow + lines[index - 1].PDFRowCount;
            TotalScenes = lines[index - 1].GlobalScene;
        }
        for (int i = index; i < lines.Count; i++)
        {
            lines[i].PDFUnrenderedCache = lines[i].IsUnrenderedPDF(priorUnrendered);

            if (lines[i].Kind == LineKind.PageBreak ||
                (i != 0 && lines[i].Kind != LineKind.Preamble && lines[i - 1].Kind == LineKind.Preamble))
            {
                TotalPDFRows = ((TotalPDFRows / LinesPerPage) + 1) * LinesPerPage;
            }

            if (lines[i].Kind == LineKind.Scene)
            {
                TotalScenes++;
            }

            lines[i].GlobalRow = TotalRows;
            lines[i].GlobalPDFRow = TotalPDFRows;
            lines[i].GlobalScene = TotalScenes;
            TotalRows += lines[i].RowCount;
            TotalPDFRows += lines[i].PDFRowCount;
            priorUnrendered = lines[i].PDFUnrenderedCache;
        }
    }

    void InternallyRecombobulate(int index)
    {
        LineKind oldKind;
        LineKind newKind = index == 0 ? LineKind.Nonexistant : lines[index - 1].Kind;
        do
        {
            oldKind = lines[index].Kind;
            lines[index].InternalRecombobulateDontCall(newKind);
            newKind = lines[index].Kind;
            index++;
        }
        while (index < lines.Count && oldKind != newKind);
    }

    // I'm not sure how much I like this kinda closure thing,
    // because it adds a weird layer of indirection, but hopefully
    // people can figure this out.
    public void Alter(int index, Action<LineMutator> action)
    {
        action(new LineMutator(lines[index]));
        Recombobulate(index);
    }

    public string Alter(int index, Func<LineMutator, string> action)
    {
        string result = action(new LineMutator(lines[index]));
        Recombobulate(index);
        return result;
    }

    public string Remove(int index)
    {
        string value = lines[index].RawContent;
        lines.RemoveAt(index);
        return value;
    }

    // Returns the new columnindex thing
    public int InsertMultilineAtCaret(int row, int index, string[] lines)
    {
        if (lines.Length == 0) return index;
        
        if (lines.Length == 1)
        {
            Alter(row, mut => mut.AddString(index, lines[0]));
            return index + lines[0].Length;
        }
        else
        {
            string after = Alter(row, mut =>
            {
                mut.AddString(index, lines[0]);
                return mut.RemoveAllAfterPosition(index + lines[0].Length);
            });
            for (int i = 1; i < lines.Length - 1; i++)
            {
                row++;
                AddLine(row, lines[i]);
            }
            AddLine(row, lines[lines.Length - 1] + after);
            return lines[lines.Length - 1].Length;
        }
    }

    public int InsertMultilineAtCaret(int row, int index, string lines)
    {
        return InsertMultilineAtCaret(row, index, lines.Split("\n"));
    }

    public virtual void Save() { }
    public virtual bool CanSave()
    {
        return false;
    }

    public virtual string Name()
    {
        return "[Scratch Document]";
    }

    public void AskSave()
    {
        if (!CanSave()) return;
        if (GUI.SaveConfirmDialog())
        {
            Save();
        }
    }
}
