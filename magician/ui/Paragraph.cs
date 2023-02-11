using Magician.Renderer;

namespace Magician.UI
{
    public class Paragraph : Multi
    {
        string[] sentences;
        Paragraph(double x = 0, double y = 0, string fontPath = "", Color? c = null, params string[] ss) : base(x, y, c ?? Data.Col.UIDefault.FG, DrawMode.INVISIBLE)
        {
            sentences = new string[ss.Length];
            // If no path given, use the default
            fontPath = fontPath == "" ? Text.FallbackFontPath : fontPath;
            for (int i = 0; i < ss.Length; i++)
            {
                sentences[i] = ss[i];
                this[$"line{i}"] = new Multi(0, -i * Data.Globals.fontSize)
                .Textured(new Text(sentences[i], col, fontPath).Render())
                .DrawFlags(DrawMode.INVISIBLE)
                ;
            }
        }
        public Paragraph(double x, double y, string s, Color c, char delimiter = '\n', string fp = "") : this(x, y, fp, c, s.Split(delimiter)) { }
        public Paragraph(double x, double y, Color? c = null, params string[] ss) : this(x, y, "", c, ss) { }
    }
}