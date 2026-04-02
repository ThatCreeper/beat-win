using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using Color = Raylib_cs.Color;

namespace beat_win;

public interface ITextEditorRenderer
{
    public bool NeedsRender { get; }

    public int Text(string text, int x, int y, bool italic, bool bold, bool underline, Color color, bool syntax);
    public void MarkDirty();
    public void MarkNotDirty();
}
