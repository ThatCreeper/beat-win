using System;
using System.Collections.Generic;
using System.Text;

namespace beat_win;

public class FileDocument : Document
{
    public string Path;
    string name;

    public FileDocument(string path) : base()
    {
        
        Path = path;
        name = System.IO.Path.GetFileName(Path);
        string[] lines = File.ReadAllLines(Path);
        InsertMultilineAtCaret(0, 0, lines);
        Edited = false;
    }

    public static FileDocument? NewWithDialog()
    {
        SaveFileDialog dialog = new();
        dialog.Filter = "Fountain Files (*.fountain, *.txt)|*.fountain;*.txt|All Files (*.*)|*.*";
        dialog.AddExtension = true;
        dialog.OverwritePrompt = true;
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            File.WriteAllText(dialog.FileName, "");
            return new FileDocument(dialog.FileName);
        }
        else
        {
            return null;
        }
    }

    public static FileDocument? OpenWithDialog()
    {
        OpenFileDialog dialog = new();
        dialog.Filter = "Fountain Files (*.fountain, *.txt)|*.fountain;*.txt|All Files (*.*)|*.*";
        dialog.CheckFileExists = true;
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            return new FileDocument(dialog.FileName);
        }
        else
        {
            return null;
        }
    }

    public override void Save()
    {
        string serialized = String.Join("\n", Lines.Select(line => line.RawContent));
        File.WriteAllText(Path, serialized);
        Edited = false;
        RenderHelper.NeedsRender = true;
    }

    public override string Name()
    {
        return name;
    }

    public override bool CanSave()
    {
        return true;
    }
}
