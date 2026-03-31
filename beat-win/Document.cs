namespace beat_win;

public class Document
{
    public IReadOnlyList<Line> Lines;
    public int TotalRows = 0;
    public int TotalPDFRows = 0;

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
            line.PDFUnrenderedCache = line.IsUnrenderedPDF(true);
        }
        else
        {
            line.GlobalRow = lines[index - 1].GlobalRow + lines[index - 1].RowCount;
            line.GlobalPDFRow = lines[index - 1].GlobalPDFRow + lines[index - 1].PDFRowCount;
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
        }
        else
        {
            TotalRows = lines[index - 1].GlobalRow + lines[index - 1].RowCount;
            TotalPDFRows = lines[index - 1].GlobalPDFRow + lines[index - 1].PDFRowCount;
        }
        for (int i = index; i < lines.Count; i++)
        {
            lines[i].PDFUnrenderedCache = lines[i].IsUnrenderedPDF(priorUnrendered);

            if (lines[i].Kind == LineKind.PageBreak)
            {
                TotalPDFRows = ((TotalPDFRows / LinesPerPage) + 1) * LinesPerPage;
            }

            lines[i].GlobalRow = TotalRows;
            lines[i].GlobalPDFRow = TotalPDFRows;
            TotalRows += lines[i].RowCount;
            TotalPDFRows += lines[i].PDFRowCount;
            priorUnrendered = lines[i].PDFUnrenderedCache;
        }
    }

    void InternallyRecombobulate(int index)
    {
        LineKind oldKind;
        LineKind newKind;
        do
        {
            oldKind = lines[index].Kind;
            lines[index].InternalRecombobulateDontCall();
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
}
