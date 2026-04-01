using Raylib_cs;

namespace beat_win;

public static class GUI
{
    public static Color Background = new(33, 33, 33);
    public static Color Foreground = new(185, 185, 185);
    public static Color Note = new(255, 162, 12);
    public static Color Syntax = new(99, 99, 99);
    public static Color UIBackground = new(18, 18, 18);
    public static Color UIForeground = new(148, 148, 148);
    public static Color Cursor = new(0, 137, 219);

    public static Font FontNormal;
    public static Font FontItalic;
    public static Font FontBold;
    public static Font FontBoldItalic;

    public static int TopPad => Inch(1f);
    public static float ActionLeftPad = 1.25f;
    public static float ActionRightPad = 1.25f;
    public static float CharacterLeftPad = ActionLeftPad + 2.5f;
    public static float ParentheticalLeftPad = ActionLeftPad + 2;
    public static float ParentheticalRightPad = ActionRightPad + 2;
    public static float DialogueLeftPad = ActionLeftPad + 1.3f;
    public static float DialogueRightPad = ActionRightPad + 1;

    public static int TextSize => Point(12);
    public static float CharacterWidth => FloatInch(0.1f);

    static float StockDPI = 96;
    static float InchToPx = StockDPI;

    public static float FloatInch(float inches) => inches * InchToPx;
    public static int Inch(float inches) => (int)FloatInch(inches);
    public static float FloatPoint(float points) => FloatInch(points / 72);
    public static int Point(float points) => Inch(points / 72);

    public static void Update()
    {
        InchToPx = Raylib.GetWindowScaleDPI().X * StockDPI;
    }

    public static Font LoadFont(string path)
    {
        Font font = Raylib.LoadFont(path);
        Raylib.SetTextureFilter(font.Texture, TextureFilter.Bilinear);
        return font;
    }

    public static void Load()
    {
        Update();

        FontNormal = LoadFont("Courier Prime.ttf");
        FontItalic = LoadFont("Courier Prime Italic.ttf");
        FontBold = LoadFont("Courier Prime Bold.ttf");
        FontBoldItalic = LoadFont("Courier Prime Bold Italic.ttf");
    }

    public static Font GetFont(bool italic, bool bold)
    {
        if (italic && bold)
            return FontBoldItalic;
        if (italic)
            return FontItalic;
        if (bold)
            return FontBold;
        return FontNormal;
    }

    public static int TextWidth(int length) => (int)(CharacterWidth * length);
    public static int TextWidth(string text) => TextWidth(text.Length);

    public static int Text(string text, int x, int y, bool italic, bool bold, bool underline, Color color)
    {
        Font font = GetFont(italic, bold);
        int size = TextSize;
        float chWid = CharacterWidth;
        int width = TextWidth(text);
        if (underline)
        {
            Raylib.DrawRectangle(x - Point(2), y + size, width + Point(4), Point(0.5f), color);
        }
        for (int i = 0; i < text.Length; i++)
        {
            Raylib.DrawTextCodepoint(font, text[i], new System.Numerics.Vector2(x + chWid * i, y), size, color);
        }
        return width;
    }
}
