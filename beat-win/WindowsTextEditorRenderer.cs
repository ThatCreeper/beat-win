using beat_win.doc;
using beat_win.forms;
using System;
using System.Collections.Generic;
using System.Text;

namespace beat_win;

public class WindowsTextEditorRenderer : ScrollAwareTextEditorRenderer
{
    public FormWindowsEditor form;
    public char CommandCharacter;

    public WindowsTextEditorRenderer()
    {
        form = new FormWindowsEditor(this);
    }

    public override IWin32Window GetWindowHandle()
    {
        return form;
    }

    public override bool IsKeyCommandCharacter(char key)
    {
        return key == CommandCharacter;
    }

    public override void RenderDocument(Document document, Caret caret)
    {
        throw new NotImplementedException();
    }

    public override void Run()
    {
        Application.Run(form);
    }

    public override int Text(string text, int x, int y, bool italic, bool bold, bool underline, Raylib_cs.Color color, bool syntax)
    {
        throw new NotImplementedException();
    }
}
