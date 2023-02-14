using Magician.Renderer;
using System.Linq;

namespace Magician.UI
{
    public class Paragraph : Multi
    {
        protected string[]? sentences;
        Paragraph(double x = 0, double y = 0, string fontPath = "", Color? c = null, int? size = null, params string[] ss) : base(x, y, c ?? Data.Col.UIDefault.FG, DrawMode.INVISIBLE)
        {
            sentences = new string[ss.Length];
            // If no path given, use the default
            fontPath = fontPath == "" ? Text.FallbackFontPath : fontPath;
            for (int i = 0; i < ss.Length; i++)
            {
                sentences[i] = ss[i];
                size = size ?? Data.Globals.fontSize;
                Text t = new Text(sentences[i], col, (int)size, fontPath);
                this[$"line{i}"] = new Multi(0, -i * (int)size)
                .Textured(t.Render())
                .DrawFlags(DrawMode.INVISIBLE)
                ;
                t.Dispose();
            }
        }
        public Paragraph(double x, double y, string s, Color c, int size, char delimiter = '\n', string fp = "") : this(x, y, fp, c, size, s.Split(delimiter)) { }
        public Paragraph(double x, double y, Color? c = null, int? size = null, params string[] ss) : this(x, y, "", c, size, ss) { }
        // Does not set text, only position and colour. Used by RichParagraph
        protected Paragraph(double x, double y, Color c) : base(x, y, c, DrawMode.INVISIBLE) { }

        /* TODO: finish these */
        public Multi LeftAligned()
        {
            throw new NotImplementedException("Doesn't work yet");
        }
        public Multi RightAligned()
        {
            throw new NotImplementedException("Doesn't work yet");
        }
        public Multi Justified()
        {
            // Determine maximum width of our lines
            throw new NotImplementedException("Doesn't work yet");
        }
    }

    /* Paragraph with in-line formatting */
    public class RichParagraph : Paragraph
    {
        int size;
        RichParagraph(double x = 0, double y = 0, string fontPath = "", Color? c = null, int? sz = null, params string[] inputStr) : base(x, y, c ?? Data.Col.UIDefault.FG)
        {
            // If no size given, use the default
            size = sz ?? Data.Globals.fontSize;

            // If no path given, use the default
            fontPath = fontPath == "" ? Text.FallbackFontPath : fontPath;

            // Group the input into text and formatters
            List<string[]> groupedFormats = new List<string[]>();
            for (int i = 0; i < inputStr.Length; i++)
            {
                // Split each line along the format delimiter
                string[] formats = inputStr[i].Split(TextFormatSetting.Delimiter);

                // Filter empties
                formats = formats.Where(s => s != "").ToArray();

                // Flag the formatters as 1 and the text as 0
                bool[] flags = formats.Select(f => f.Substring(0, 1) == TextFormatSetting.Prefix.ToString()).ToArray();

                // Remove extraneous format settings
                bool lastFlag = false;
                int run = 0;
                List<string> filteredFormats = new List<string>();
                for (int j = 0; j < formats.Length; j++)
                {
                    if (lastFlag && flags[j] == lastFlag)
                    {
                        filteredFormats[j - ++run] = formats[j];
                        continue;
                    }
                    run = 0;
                    filteredFormats.Add(formats[j]);
                    lastFlag = flags[j];
                }
                groupedFormats.Add(filteredFormats.ToArray());
            }

            // Assemble this bad boy
            Color currentCol = col;
            int currentSize = size;
            int maxSize = currentSize;
            Multi lineByLine = new Multi().DrawFlags(DrawMode.INVISIBLE);
            for (int row = 0; row < groupedFormats.Count; row++)
            {
                Multi wordsInLine = new Multi().DrawFlags(DrawMode.INVISIBLE);
                int runningLength = 0;
                for (int col = 0; col < groupedFormats[row].Length; col++)
                {
                    char ch = groupedFormats[row][col][0];
                    // Is it a formatter?
                    if (ch == TextFormatSetting.Prefix)
                    {
                        string formatSettings = groupedFormats[row][col].Substring(1);
                        // Extract the formatters
                        foreach (string formatSetting in formatSettings.Split(TextFormatSetting.Prefix))
                        {
                            // What kind of formatter is it?
                            char formatSettingIdentifer = formatSetting[0];
                            switch (formatSettingIdentifer)
                            {
                                case ((char)TextFormatSetting.FormatSetting.COLOR):
                                    byte[] rgba = Convert.FromHexString(formatSetting.Substring(1));
                                    currentCol = new RGBA(rgba[0], rgba[1], rgba[2], rgba[3]);
                                    break;
                                case ((char)TextFormatSetting.FormatSetting.SIZE):
                                    int.TryParse(formatSetting.Substring(1), out currentSize);
                                    maxSize = currentSize > maxSize ? currentSize : maxSize;
                                    break;
                                default:
                                    Scribe.Error($"Unsupported format setting identifer {formatSettingIdentifer}");
                                    break;
                            }
                        }
                    }
                    // Is it a phrase?
                    else
                    {
                        // Add words
                        string[] words = groupedFormats[row][col].ToString().Split(' ');
                        Text t = new Text(groupedFormats[row][col].ToString(), currentCol, currentSize);
                        Texture txr = t.Render();

                        lineByLine.Add(new Multi(runningLength, -row * maxSize - (maxSize - currentSize)).Textured(txr));
                        runningLength += txr.Width;
                        t.Dispose();
                    }
                }
            }
            // Become the assembled Multi
            Become(lineByLine);
        }
        public RichParagraph(double x, double y, string s, Color c, int size, char delimiter = '\n', string fp = "") : this(x, y, fp, c, size, s.Split(delimiter)) { }
        public RichParagraph(double x, double y, Color? c = null, int? size = null, params string[] ss) : this(x, y, "", c, size, ss) { }
    }
}