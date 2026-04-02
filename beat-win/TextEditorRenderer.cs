using beat_win.doc;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Text;
using Color = Raylib_cs.Color;

namespace beat_win;

public abstract class TextEditorRenderer
{
    public bool NeedsRender => needsRender;
    public Action<string>? OnDroppedFile;
    public Action? OnRequestRender;
    public Action<float>? OnScrollEvent;
    public Action<int, int>? OnClickOnDocument;
    public Action<int, int>? OnDragOnDocument;
    public Action<string>? OnInputCharacter;
    public Action? OnInputCommandCharacter;
    public Action? OnEnter;
    public Action? OnTab;
    public Action? OnBackspace;
    public Action<bool>? OnLeft; // Shifting?
    public Action<bool>? OnRight;
    public Action<bool>? OnUp;
    public Action<bool>? OnDown;

    bool needsRender = true;

    public void MarkDirty()
    {
        needsRender = true;
    }
    public void MarkNotDirty()
    {
        needsRender = false;
    }

    public abstract void Run();
    public abstract int Text(string text, int x, int y, bool italic, bool bold, bool underline, Color color, bool syntax);
    public abstract void RenderDocument(Document document, Caret caret);
    public abstract bool IsKeyCommandCharacter(char key);
    public abstract IntPtr GetWindowHandle();
}
