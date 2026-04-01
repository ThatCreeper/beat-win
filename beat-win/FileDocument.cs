using System;
using System.Collections.Generic;
using System.Text;

namespace beat_win;

public class FileDocument : Document
{
    public string Path;

    public FileDocument(string path) : base()
    {
        
        Path = path;
        string[] lines = File.ReadAllLines(Path);
        InsertMultilineAtCaret(0, 0, lines);
    }

    public override void Save()
    {
        string serialized = String.Join("\n", Lines.Select(line => line.RawContent));
        File.WriteAllText(Path, serialized);
    }
}
