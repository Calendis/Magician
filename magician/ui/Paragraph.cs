using Magician.Renderer;
using System.Linq;

namespace Magician.UI
{
    public class Paragraph : Multi
    {
        protected string[]? sentences;
        Paragraph(double x = 0, double y = 0, string fontPath = "", Color? c = null, params string[] ss) : base(x, y, c ?? Data.Col.UIDefault.FG, DrawMode.INVISIBLE)
        {
            sentences = new string[ss.Length];
            // If no path given, use the default
            fontPath = fontPath == "" ? Text.FallbackFontPath : fontPath;
            for (int i = 0; i < ss.Length; i++)
            {
                sentences[i] = ss[i];
                Text t = new Text(sentences[i], col, fontPath);
                this[$"line{i}"] = new Multi(0, -i * Data.Globals.fontSize)
                .Textured(t.Render())
                .DrawFlags(DrawMode.INVISIBLE)
                ;
                t.Dispose();
            }
        }
        public Paragraph(double x, double y, string s, Color c, char delimiter = '\n', string fp = "") : this(x, y, fp, c, s.Split(delimiter)) { }
        public Paragraph(double x, double y, Color? c = null, params string[] ss) : this(x, y, "", c, ss) { }
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
        RichParagraph(double x = 0, double y = 0, string fontPath = "", Color? c = null, params string[] inputStr) : base(x, y, c ?? Data.Col.UIDefault.FG)
        {
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

                /* this[$"line{i}"] = new Multi(0, -i * Data.Globals.fontSize)
                .Textured(new Text(inputStr[i], col, fontPath).Render())
                .DrawFlags(DrawMode.INVISIBLE)
                ; */
            }

            // Assemble this bad boy
            Color currentCol = col;
            double currentSize;
            Multi lineByLine = new Multi().DrawFlags(DrawMode.INVISIBLE);
            for (int row = 0; row < groupedFormats.Count; row++)
            {
                Multi wordsInLine = new Multi().DrawFlags(DrawMode.INVISIBLE);
                // Scan and break up the line if formatters are there
                int runningLength = 0;
                for (int col = 0; col < groupedFormats[row].Length; col++)
                {
                    char ch = groupedFormats[row][col][0];
                    if (ch == TextFormatSetting.Prefix)
                    {
                        char formatSettingIdentifer = groupedFormats[row][col][1];
                        switch (formatSettingIdentifer)
                        {
                            case ((char)TextFormatSetting.FormatSetting.COLOR):
                            {
                                currentCol = HSLA.RandomVisible();
                                string colorSetting = groupedFormats[row][col].Substring(2);
                                byte[] rgba = Convert.FromHexString(colorSetting);
                                currentCol = new RGBA(rgba[0], rgba[1], rgba[2], rgba[3]);
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Add words
                        string[] words = groupedFormats[row][col].ToString().Split(' ');
                        
                        foreach (string word in words)
                        {
                            if (word == "")
                            {
                                continue;
                            }
                            string paddedWord = word + " ";
                            Text t = new Text(paddedWord, currentCol, fontPath);
                            Texture txr = t.Render();
                            wordsInLine[$"{row}{col}{paddedWord.Substring(0, Math.Min(16, paddedWord.Length - 1))}"] = new Multi()
                            .Textured(txr)
                            .Positioned(runningLength, -row * Data.Globals.fontSize)
                            ;
                            t.Dispose();
                            runningLength += txr.Width;
                        }
                    }
                }
                lineByLine[$"line{row}"] = wordsInLine;
            }
            Become(lineByLine);
        }
        public RichParagraph(double x, double y, string s, Color c, char delimiter = '\n', string fp = "") : this(x, y, fp, c, s.Split(delimiter)) { }
        public RichParagraph(double x, double y, Color? c = null, params string[] ss) : this(x, y, "", c, ss) { }
    }
}